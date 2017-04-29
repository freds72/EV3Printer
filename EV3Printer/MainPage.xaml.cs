using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EV3Printer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CanvasBitmap _bitmap;
        Vector2 _bitmapCenter;
        Rect _bitmapBounds;

        const int simulationW = 512;
        const int simulationH = 512;

        Transform2DEffect transformEffect;
        CanvasRenderTarget currentSurface, nextSurface;
        static TimeSpan normalTargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30.0);
        static TimeSpan slowTargetElapsedTime = TimeSpan.FromSeconds(0.25);

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void canvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            // Swap the current and next surfaces.
            var tmp = currentSurface;
            currentSurface = nextSurface;
            nextSurface = tmp;
        }

        static Matrix3x2 GetDisplayTransform(ICanvasAnimatedControl canvas)
        {
            var outputSize = canvas.Size.ToVector2();
            var sourceSize = new Vector2(canvas.ConvertPixelsToDips(simulationW), canvas.ConvertPixelsToDips(simulationH));

            return CanvasUtils.GetDisplayTransform(outputSize, sourceSize);
        }

        private void canvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            Draw();

            // Display the current surface.
            transformEffect.Source = currentSurface;
            transformEffect.TransformMatrix = GetDisplayTransform(sender);
            args.DrawingSession.DrawImage(transformEffect);
        }

        private void Draw()
        {
            using (var ds = currentSurface.CreateDrawingSession())
            {
                ds.Clear(Colors.Black);
                ds.Transform =
                    Matrix3x2.Multiply(
                        Matrix3x2.CreateScale(new Vector2(1, -1)),
                        Matrix3x2.CreateTranslation(new Vector2(simulationW / 2, simulationH / 2)));

            }
        }

        private void canvas_CreateResources(CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            if (args.Reason == CanvasCreateResourcesReason.DpiChanged)
                return;

            const float defaultDpi = 96;

            currentSurface = new CanvasRenderTarget(sender, simulationW, simulationH, defaultDpi);
            nextSurface = new CanvasRenderTarget(sender, simulationW, simulationH, defaultDpi);

            transformEffect = new Transform2DEffect
            {
                Source = currentSurface,
                InterpolationMode = CanvasImageInterpolation.NearestNeighbor,
            };

            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {         
        }

        private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {         
        }

        async Task CreateResourcesAsync(CanvasAnimatedControl sender)
        {
            await CreateBitmapResourcesAsync(sender);
        }

        async Task CreateBitmapResourcesAsync(CanvasAnimatedControl sender)
        {
            _bitmap = await CanvasBitmap.LoadAsync(sender, "Assets/blank.png");

            _bitmapCenter = _bitmap.Size.ToVector2() / 2;
            _bitmapBounds = _bitmap.Bounds;
        }
    }
}
