using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDX.Mathematics.Interop;
using DXDevice = SharpDX.Direct3D11.Device;
using rqdq.app;

namespace rqdq {
namespace app {

internal static
class MyApp {

  public static string? DataDir;
  public static string? SceneFileName;

  static public
  int Main(string[] args) {
    DataDir = System.Environment.GetEnvironmentVariable("RQDQ__VIEWNG__DATA_DIR") ?? "data";
    SceneFileName = System.Environment.GetEnvironmentVariable("RQDQ__VIEWNG__SCENE_FILE_NAME") ?? "scene.json";
    AnyCompiler.Setup();
    return AsyncMain(args).GetAwaiter().GetResult(); }

  static public
  async Task<int> AsyncMain(string[] args) {
    SystemValues systemValues = new("system");
    List<Node> builtins = new() { systemValues };
    var clock = Stopwatch.StartNew();


    var form = new RenderForm("rqdq 2022");
    var desc = new SwapChainDescription() {
      BufferCount = 1,
      ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                            new Rational(60, 1), Format.R8G8B8A8_UNorm),
      IsWindowed = true,
      OutputHandle = form.Handle,
      SampleDescription = new SampleDescription(1, 0),
      SwapEffect = SwapEffect.Discard,
      Usage = Usage.RenderTargetOutput
      };

    DXDevice.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out DXDevice device, out SwapChain swapChain);
    var dc = device.ImmediateContext;

    var factory = swapChain.GetParent<Factory>();
    factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

    var mtime = File.GetLastWriteTime(Path.Join(DataDir, SceneFileName));
    string sceneText = File.ReadAllText(Path.Join(DataDir, SceneFileName));
    var runningGraph = Compile(sceneText, builtins);
    if (runningGraph.root is null) {
      throw new Exception("did not find a layer node with id __main__"); }
    runningGraph.Init(device);
    try {

      var modifyTimer = Stopwatch.StartNew();

      bool userResized = true;
      Texture2D backBuffer = null;
      RenderTargetView renderView = null;
      Texture2D depthBuffer = null;
      DepthStencilView depthView = null;

      form.UserResized += (send, args) => userResized = true;

      form.KeyUp += (sender, args) => {
        if (args.KeyCode == Keys.F5) {
          swapChain.SetFullscreenState(true, null); }
        else if (args.KeyCode == Keys.F4) {
          swapChain.SetFullscreenState(false, null); }
        else if (args.KeyCode == Keys.Escape) {
          form.Close(); }};

      RenderLoop.Run(form, () => {
        if (userResized) {
          Utilities.Dispose(ref backBuffer);
          Utilities.Dispose(ref renderView);
          Utilities.Dispose(ref depthBuffer);
          Utilities.Dispose(ref depthView);

          swapChain.ResizeBuffers(desc.BufferCount,
                                  form.ClientSize.Width,
                                  form.ClientSize.Height,
                                  Format.Unknown,
                                  SwapChainFlags.None);

          backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
          renderView = new RenderTargetView(device, backBuffer);
          depthBuffer = new Texture2D(device, new Texture2DDescription() {
            Format = Format.D32_Float_S8X24_UInt,
            ArraySize = 1,
            MipLevels = 1,
            Width = form.ClientSize.Width,
            Height = form.ClientSize.Height,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.DepthStencil,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None, });
          depthView = new DepthStencilView(device, depthBuffer);
          dc.Rasterizer.SetViewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0F, 1.0F);
          dc.OutputMerger.SetTargets(depthView, renderView);
          userResized = false; }

        systemValues.Upsert("T", clock.ElapsedMilliseconds / 1000.0F);
        systemValues.Upsert("canvasSize", new Vector2(form.ClientSize.Width, form.ClientSize.Height));
        systemValues.Upsert("pixelAspect", new Vector2(1, 1));

        dc.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0F, 0);

        if (runningGraph.root is ILayer rootLayer) {
          var tmp = rootLayer.GetColor();
          dc.ClearRenderTargetView(renderView, new RawColor4(tmp.X, tmp.Y, tmp.Z, 1.0F));
          rootLayer.Draw(dc); }

        swapChain.Present(0, PresentFlags.None);

        if (modifyTimer.ElapsedMilliseconds > 250) {
          modifyTimer.Restart();

          var mt = File.GetLastWriteTime(Path.Join(DataDir, SceneFileName));
          if (mt > mtime) {
            Console.WriteLine($"change detected: {mt}");
            mtime = mt;
            SceneGraph? newGraph = null;
            try {
              string sceneText = File.ReadAllText(Path.Join(DataDir, SceneFileName));
              newGraph = Compile(sceneText, builtins);
              if (newGraph?.root is null) {
                throw new Exception("did not find a layer node with id __main__"); }}
            catch (Exception err) {
              Console.WriteLine(err.Message);
              newGraph = null; }

            if (newGraph is not null) {
              newGraph.Init(device);
              try {
                (runningGraph, newGraph) = (newGraph, runningGraph); }
              finally {
                newGraph.Dispose(); }}}}

        });

        depthBuffer.Dispose();
        depthView.Dispose();
        renderView.Dispose();
        backBuffer.Dispose();

      }

    finally {
      runningGraph.Dispose(); }

    dc.ClearState();
    dc.Flush();
    device.Dispose();
    dc.Dispose();
    swapChain.Dispose();
    factory.Dispose();
    return 0; }

  static
  SceneGraph Compile(string text, List<Node> addl) {
    SceneGraph sg = new();
    CompileResult cr;
    using (JsonDocument doc = JsonDocument.Parse(text)) {
      var docroot = doc.RootElement;
      foreach (var elem in docroot.EnumerateArray()) {
        cr = AnyCompiler.Compile(elem);
        if (cr.root is not null) {
          sg.node.AddRange(cr.nodes);
          sg.link.AddRange(cr.links); }
        else {
          throw new Exception("compile failed list"); }}}

    sg.node.AddRange(addl);
    GraphLinker.Link(sg);
    return sg; } }

}  // close package namespace
}  // close enterprise namespace
