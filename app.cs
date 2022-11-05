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

namespace rqdq {
namespace app {

internal static
class MyApp {

  public const string dataDir = @"c:\users\ben\src\rsr\data\mesh";

  static public
  int Main(string[] args) {

    // FloatNodeCompiler.Install();
    // Float2NodeCompiler.Install();
    Float3NodeCompiler.Install();
    // Float4NodeCompiler.Install();
    MulNodeCompiler.Install();
    AddNodeCompiler.Install();
    GlRotateCompiler.Install();
    GlMultiplyCompiler.Install();
    GlLayerCompiler.Install();
    GlMeshCompiler.Install();
    LookAtCompiler.Install();

    int result = AsyncMain(args).GetAwaiter().GetResult();
    return result; }

  static public
  async Task<int> AsyncMain(string[] args) {
    List<Node> graphNodes = new();
    List<NodeLink> graphLinks = new();

    List<Node> builtins = new();
    SystemValues systemValues = new("system");
    builtins.Add(systemValues);

    string data = @"[
{""$layer"": {
   ""id"": ""__main__"",
   ""camera"": {""$look"": {
                  ""position"": [0, 5, -5],
                  ""target"": [0,0,0],
                  ""aspect"": ""system:canvasSize""
                  }},
   ""color"": [0.1, 0.1, 0.1],
   ""gl"": {""$rotate"": {
              ""amount"": {""$mul"": {
                             ""a"": [0, 0.5, 0],
                             ""b"": ""system:T""
                             }},
              ""gl"": {""$multiply"": {""many"": [4,0,0],
                                       ""rotate"": [3.14,0,0],
                                       ""translate"": [2,0,0],
                                       ""gl"": {""$mesh"": {""src"": ""colortest.obj""}} }}
              }}
   }}]";

    CompileResult cr;
    using (JsonDocument doc = JsonDocument.Parse(data)) {
      var docroot = doc.RootElement;
      foreach (var elem in docroot.EnumerateArray()) {
        cr = AnyCompiler.Compile(elem);
        if (cr.root is not null) {
          graphNodes.AddRange(cr.nodes);
          graphLinks.AddRange(cr.links); }
        else {
          throw new Exception("compile failed list"); }}}

    graphNodes.AddRange(builtins);
    GraphLinker.Link(graphNodes, graphLinks);

    ILayer? sceneRoot = null;
    foreach (var node in graphNodes) {
      if (node.Id == "__main__") {
        if (node is ILayer layerNode) {
          sceneRoot = layerNode; }}}
    if (sceneRoot is null) {
      throw new Exception("did not find a layer node with id __main__"); }


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

    foreach (var node in graphNodes) {
      node.Init(device); }
        

    var clock = Stopwatch.StartNew();

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

      var layer = (ILayer)sceneRoot;
      var tmp = layer.GetColor();
      dc.ClearRenderTargetView(renderView, new RawColor4(tmp.X, tmp.Y, tmp.Z, 1.0F));
      layer.Draw(dc);

      swapChain.Present(0, PresentFlags.None); });
      
    // XXX dispose on all nodes? scene.Dispose();
    depthBuffer.Dispose();
    depthView.Dispose();
    renderView.Dispose();
    backBuffer.Dispose();
    dc.ClearState();
    dc.Flush();
    device.Dispose();
    dc.Dispose();
    swapChain.Dispose();
    factory.Dispose();
    return 0; } }


}  // close package namespace
}  // close enterprise namespace
