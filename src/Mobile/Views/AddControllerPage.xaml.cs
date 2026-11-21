using Mobile.ViewModels;
using ZXing.Net.Maui;

namespace Mobile.Views;

public partial class AddControllerPage : ContentPage
{
    public AddControllerPage(AddControllerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Configure BarcodeReader options
        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.All,
            AutoRotate = true,
            Multiple = false,
            TryHarder = true
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AddControllerViewModel vm)
        {
            await RequestCameraPermissionAsync(vm);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is AddControllerViewModel vm)
        {
            vm.IsDetecting = false;
            vm.IsTorchOn = false;
        }
    }

    private async Task RequestCameraPermissionAsync(AddControllerViewModel vm)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            vm.IsCameraPermissionGranted = (status == PermissionStatus.Granted);
            if (vm.IsCameraPermissionGranted && !vm.IsQrScanned && vm.IsQrMode)
            {
                vm.IsDetecting = true;
            }
        }
        catch
        {
            // On desktop/windows or restricted platforms, graceful fallback
            vm.IsCameraPermissionGranted = true;
            if (!vm.IsQrScanned && vm.IsQrMode)
            {
                vm.IsDetecting = true;
            }
        }
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results?.FirstOrDefault();
        if (first == null || string.IsNullOrWhiteSpace(first.Value))
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (BindingContext is AddControllerViewModel vm)
            {
                vm.HandleScannedBarcode(first.Value);
            }
        });
    }
}
