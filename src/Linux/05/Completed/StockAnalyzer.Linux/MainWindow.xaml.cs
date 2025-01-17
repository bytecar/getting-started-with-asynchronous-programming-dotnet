﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Newtonsoft.Json;
using StockAnalyzer.Core.Domain;
using StockAnalyzer.Linux.Services;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer.Linux
{
    public partial class MainWindow
    {
        public async Task Run()
        {
            await RunInternal(async () =>
            {
                await Task.Delay(200);
            });
        }

        public async Task RunInternal(Action action)
        {
            await Task.Delay(100).ContinueWith(_ => action());
        }

        #region Asynchronous Streams
        CancellationTokenSource cancellationTokenSource = null;

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            #region Before loading stock data
            var watch = new Stopwatch();
            watch.Start();
            StockProgress.IsVisible = true;
            StockProgress.IsIndeterminate = true;
            Search.Content = "Cancel";
            #endregion

            #region Cancellation
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Token.Register(() =>
            {
                Notes.Text += "Cancellation requested" + Environment.NewLine;
            });
            #endregion

            try
            {
                var tickers = Ticker.Text.Split(' ');

                var prices = new ObservableCollection<StockPrice>();

                Stocks.Items = prices;

                var service = new StockDiskStreamService();

                await foreach (var price in service.GetAllStockPrices(cancellationTokenSource.Token))
                {
                    if (tickers.Contains(price.Ticker))
                    {
                        prices.Add(price);
                    }
                }
            }
            catch (Exception ex)
            {
                Notes.Text += ex.Message + Environment.NewLine;
            }

            #region After stock data is loaded
            StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
            StockProgress.IsVisible = false;
            Search.Content = "Search";
            #endregion
        }

        #endregion

        #region Task Completion Source
        //CancellationTokenSource cancellationTokenSource = null;

        //private async void Search_Click(object sender, RoutedEventArgs e)
        //{
        //    #region Before loading stock data
        //    var watch = new Stopwatch();
        //    watch.Start();
        //    StockProgress.IsVisible = true;
        //    StockProgress.IsIndeterminate = true;
        //    Search.Content = "Cancel";
        //    #endregion

        //    #region Cancellation
        //    if (cancellationTokenSource != null)
        //    {
        //        cancellationTokenSource.Cancel();
        //        cancellationTokenSource = null;
        //        return;
        //    }

        //    cancellationTokenSource = new CancellationTokenSource();
        //    cancellationTokenSource.Token.Register(() =>
        //    {
        //        Notes.Text += "Cancellation requested" + Environment.NewLine;
        //    });
        //    #endregion

        //    try
        //    {
        //        await WorkInNotepad();

        //        Notes.Text += "Notepad closed, continuation!";
        //    }
        //    catch (Exception ex)
        //    {
        //        Notes.Text += ex.Message + Environment.NewLine;
        //    }
        //    finally
        //    {
        //        cancellationTokenSource = null;
        //    }

        //    #region After stock data is loaded
        //    StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
        //    StockProgress.IsVisible = false;
        //    Search.Content = "Search";
        //    #endregion
        //}

        //public Task<IEnumerable<StockPrice>> GetStocksFor(string ticker)
        //{
        //    var source = new TaskCompletionSource<IEnumerable<StockPrice>>();

        //    ThreadPool.QueueUserWorkItem(_ =>
        //    {
        //        try
        //        {
        //            var prices = new List<StockPrice>();

        //            var lines = File.ReadAllLines(@"StockPrices_Small.csv");

        //            foreach (var line in lines.Skip(1))
        //            {
        //                var segments = line.Split(',');

        //                for (var i = 0; i < segments.Length; i++) segments[i] = segments[i].Trim('\'', '"');
        //                var price = new StockPrice
        //                {
        //                    Ticker = segments[0],
        //                    TradeDate = DateTime.ParseExact(segments[1], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
        //                    Volume = Convert.ToInt32(segments[6], CultureInfo.InvariantCulture),
        //                    Change = Convert.ToDecimal(segments[7], CultureInfo.InvariantCulture),
        //                    ChangePercent = Convert.ToDecimal(segments[8], CultureInfo.InvariantCulture),
        //                };
        //                prices.Add(price);
        //            }

        //            source.SetResult(prices.Where(price => price.Ticker == ticker));
        //        }
        //        catch (Exception ex)
        //        {
        //            source.SetException(ex);
        //        }
        //    });

        //    return source.Task;
        //}

        //public Task WorkInNotepad()
        //{
        //    var source = new TaskCompletionSource<object>();
        //    var process = new Process
        //    {
        //        EnableRaisingEvents = true,
        //        StartInfo = new ProcessStartInfo("/Applications/Notes.app/Contents/Linux/Notes")
        //        {
        //            RedirectStandardError = false,
        //            UseShellExecute = false
        //        }
        //    };

        //    process.Exited += (sender, e) =>
        //    {
        //        source.SetResult(null);
        //    };

        //    process.Start();
        //    return source.Task;
        //}
        #endregion

        #region Progress Reporting
        //CancellationTokenSource cancellationTokenSource = null;

        //private async void Search_Click(object sender, RoutedEventArgs e)
        //{
        //    #region Before loading stock data
        //    var watch = new Stopwatch();
        //    watch.Start();
        //    StockProgress.IsVisible = true;

        //    Search.Content = "Cancel";
        //    #endregion

        //    #region Cancellation
        //    if (cancellationTokenSource != null)
        //    {
        //        cancellationTokenSource.Cancel();
        //        cancellationTokenSource = null;
        //        return;
        //    }

        //    cancellationTokenSource = new CancellationTokenSource();
        //    cancellationTokenSource.Token.Register(() =>
        //    {
        //        Notes.Text += "Cancellation requested" + Environment.NewLine;
        //    });
        //    #endregion

        //    try
        //    {
        //        StockProgress.IsIndeterminate = false;
        //        StockProgress.Value = 0;
        //        StockProgress.Maximum = Ticker.Text.Split(',', ' ').Count();

        //        var progress = new Progress<IEnumerable<StockPrice>>();
        //        progress.ProgressChanged += (_, stocks) =>
        //        {
        //            StockProgress.Value += 1;
        //            Notes.Text += $"Loaded {stocks.Count()} for {stocks.First().Ticker} " +
        //            $"{Environment.NewLine}";
        //        };

        //        await LoadStocks(progress);
        //    }
        //    catch (Exception ex)
        //    {
        //        Notes.Text += ex.Message + Environment.NewLine;
        //    }
        //    finally
        //    {
        //        cancellationTokenSource = null;
        //    }

        //    #region After stock data is loaded
        //    StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
        //    StockProgress.IsVisible = false;
        //    Search.Content = "Search";
        //    #endregion
        //}

        //private async Task LoadStocks(IProgress<IEnumerable<StockPrice>> progress = null)
        //{
        //    var tickers = Ticker.Text.Split(',', ' ');

        //    var service = new StockService();

        //    var tickerLoadingTasks = new List<Task<IEnumerable<StockPrice>>>();

        //    foreach (var ticker in tickers)
        //    {
        //        var loadTask = service.GetStockPricesFor(ticker, cancellationTokenSource.Token);

        //        loadTask = loadTask.ContinueWith(stockTask =>
        //        {
        //            progress?.Report(stockTask.Result);
        //            return stockTask.Result;
        //        });

        //        tickerLoadingTasks.Add(loadTask);
        //    }

        //    var allStocks = await Task.WhenAll(tickerLoadingTasks);

        //    Stocks.Items = allStocks.SelectMany(stocks => stocks);
        //}
        #endregion

        #region Deadlock hack
        //CancellationTokenSource cancellationTokenSource = null;

        //private async void Search_Click(object sender, RoutedEventArgs e)
        //{
        //    #region Before loading stock data
        //    var watch = new Stopwatch();
        //    watch.Start();
        //    StockProgress.IsVisible = true;

        //    Search.Content = "Cancel";
        //    #endregion

        //    #region Cancellation
        //    if (cancellationTokenSource != null)
        //    {
        //        cancellationTokenSource.Cancel();
        //        cancellationTokenSource = null;
        //        return;
        //    }

        //    cancellationTokenSource = new CancellationTokenSource();
        //    cancellationTokenSource.Token.Register(() =>
        //    {
        //        Notes.Text += "Cancellation requested" + Environment.NewLine;
        //    });
        //    #endregion

        //    try
        //    {
        //        Task.Run(() => LoadStocks()).Wait();
        //    }
        //    catch (Exception ex)
        //    {
        //        Notes.Text += ex.Message + Environment.NewLine;
        //    }
        //    finally
        //    {
        //        cancellationTokenSource = null;
        //    }

        //    #region After stock data is loaded
        //    StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
        //    StockProgress.IsVisible = false;
        //    Search.Content = "Search";
        //    #endregion

        //}


        //private async Task LoadStocks()
        //{
        //    var tickers = Ticker.Text.Split(',', ' ');

        //    var service = new StockService();

        //    var tickerLoadingTasks = new List<Task<IEnumerable<StockPrice>>>();

        //    foreach (var ticker in tickers)
        //    {
        //        var loadTask = service.GetStockPricesFor(ticker, cancellationTokenSource.Token);

        //        tickerLoadingTasks.Add(loadTask);
        //    }

        //    var allStocks = await Task.WhenAll(tickerLoadingTasks);

        //    Stocks.Items = allStocks.SelectMany(stocks => stocks);
        //}

        #endregion

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
        }

        public static void Open(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }

    public partial class MainWindow : Window
    {
        public DataGrid Stocks => this.FindControl<DataGrid>(nameof(Stocks));
        public ProgressBar StockProgress => this.FindControl<ProgressBar>(nameof(StockProgress));
        public TextBox Ticker => this.FindControl<TextBox>(nameof(Ticker));
        public Button Search => this.FindControl<Button>(nameof(Search));
        public TextBox Notes => this.FindControl<TextBox>(nameof(Notes));
        public TextBlock StocksStatus => this.FindControl<TextBlock>(nameof(StocksStatus));
        public TextBlock DataProvidedBy => this.FindControl<TextBlock>(nameof(DataProvidedBy));
        public TextBlock IEX => this.FindControl<TextBlock>(nameof(IEX));
        public TextBlock IEX_Terms => this.FindControl<TextBlock>(nameof(IEX_Terms));

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            IEX.PointerPressed += (e, a) => Open("https://iextrading.com/developer/");
            IEX_Terms.PointerPressed += (e, a) => Open("https://iextrading.com/api-exhibit-a/");
        }
    }

    class StockCalculation
    {
        public string Ticker { get; set; }
        public decimal Result { get; set; }
    }
}
