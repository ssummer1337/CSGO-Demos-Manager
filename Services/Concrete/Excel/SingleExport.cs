using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
using NPOI.SS.UserModel;
using Services.Concrete.Analyzer;
using Services.Concrete.Excel.Sheets.Single;
using Services.Exceptions.Export;
using Services.Interfaces;

namespace Services.Concrete.Excel
{
    public class SingleExport : AbstractExport
    {
        private readonly ICacheService _cacheService;

        private GeneralSheet _generalSheet;

        private PlayersSheet _playersSheet;

        private RoundsSheet _roundsSheet;

        private EntryHoldKillsRoundSheet _entryHoldKillsRoundSheet;

        private EntryHoldKillsPlayerSheet _entryHoldKillsPlayerSheet;

        private EntryHoldKillsTeamSheet _entryHoldKillsTeamSheet;

        private EntryKillsRoundSheet _entryKillsRoundSheet;

        private EntryKillsPlayerSheet _entryKillsPlayerSheet;

        private EntryKillsTeamSheet _entryKillsTeamSheet;

        private KillsSheet _killsSheet;

        private KillMatrixSheet _killMatrixSheet;

        private FlashMatrixPlayersSheet _flashMatrixPlayersSheet;

        private FlashMatrixTeamsSheet _flashMatrixTeamsSheet;
        private readonly SingleExportConfiguration _configuration;

        public SingleExport(SingleExportConfiguration configuration)
        {
            _configuration = configuration;
            _cacheService = new CacheService();
        }

        public override async Task<IWorkbook> Generate()
        {
            CancellationToken cancellationToken = _configuration.CancellationToken.Token;
            Demo demo = DemoAnalyzer.ParseDemoHeader(_configuration.DemoPath);
            if (demo == null)
            {
                throw new InvalidDemoException();
            }

            if (_configuration.ForceAnalyze || !_cacheService.HasDemoInCache(demo.Id))
            {
                try
                {
                    _configuration.OnAnalyzeStart?.Invoke();
                    DemoAnalyzer analyzer = DemoAnalyzer.Factory(demo);
                    if (_configuration.Source != null)
                    {
                        demo.Source = _configuration.Source;
                    }

                    demo = await analyzer.AnalyzeDemoAsync(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    await _cacheService.WriteDemoDataCache(demo);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        throw ex;
                    }

                    throw new AnalyzeException(ex);
                }
            }
            else
            {
                demo = await _cacheService.GetDemoDataFromCache(demo.Id);
                demo.WeaponFired = await _cacheService.GetDemoWeaponFiredAsync(demo);
                demo.PlayerBlinded = await _cacheService.GetDemoPlayerBlindedAsync(demo);
                cancellationToken.ThrowIfCancellationRequested();
            }

            _generalSheet = new GeneralSheet(Workbook, demo);
            _generalSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _playersSheet = new PlayersSheet(Workbook, demo);
            _playersSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _roundsSheet = new RoundsSheet(Workbook, demo);
            _roundsSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _killsSheet = new KillsSheet(Workbook, demo);
            _killsSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _entryHoldKillsRoundSheet = new EntryHoldKillsRoundSheet(Workbook, demo);
            _entryHoldKillsRoundSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _entryHoldKillsPlayerSheet = new EntryHoldKillsPlayerSheet(Workbook, demo);
            _entryHoldKillsPlayerSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _entryHoldKillsTeamSheet = new EntryHoldKillsTeamSheet(Workbook, demo);
            _entryHoldKillsTeamSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _entryKillsRoundSheet = new EntryKillsRoundSheet(Workbook, demo);
            _entryKillsRoundSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _entryKillsPlayerSheet = new EntryKillsPlayerSheet(Workbook, demo);
            _entryKillsPlayerSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _entryKillsTeamSheet = new EntryKillsTeamSheet(Workbook, demo);
            _entryKillsTeamSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _killMatrixSheet = new KillMatrixSheet(Workbook, demo);
            _killMatrixSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _flashMatrixPlayersSheet = new FlashMatrixPlayersSheet(Workbook, demo);
            _flashMatrixPlayersSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();
            _flashMatrixTeamsSheet = new FlashMatrixTeamsSheet(Workbook, demo);
            _flashMatrixTeamsSheet.Generate();
            cancellationToken.ThrowIfCancellationRequested();

            return Workbook;
        }
    }
}
