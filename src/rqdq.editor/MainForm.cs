using System.Diagnostics;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Numerics;
using SharpDX.Mathematics.Interop;

namespace rqdq.editor {

public partial
class MainForm : Form {
  private SharpDX.Direct3D11.Device? _gpu;
  private SwapChainDescription _desc;
  private SwapChain? _sc;
  private Texture2D? _renderTarget;
  private Texture2D? _depthBuffer;
  private RenderTargetView? _renderTargetView;
  private DepthStencilView? _depthBufferView;
  private bool _renderResized = false;

  private readonly string _dataDir;
  private readonly string _sceneFileName;
  private readonly scene.SystemValues _systemValues = new("system");
  private readonly List<scene.Node> _builtins = new();
  private readonly Stopwatch _clock = Stopwatch.StartNew();

  private DateTime _mtime;
  private scene.SceneGraph? _runningGraph;

  public
  MainForm() {
    _dataDir = System.Environment.GetEnvironmentVariable("RQDQ__VIEWNG__DATA_DIR") ?? "data";
    _sceneFileName = System.Environment.GetEnvironmentVariable("RQDQ__VIEWNG__SCENE_FILE_NAME") ?? "scene.json";
    scene.AnyCompiler.Setup();
    scene.GlMeshCompiler.DataDir = _dataDir;  // XXX yuck

    _builtins.Add(_systemValues);
    InitializeComponent(); }

  private void MainForm_Load(object sender, EventArgs e) {
    _desc = new() {
      BufferCount = 1,
      Flags = SwapChainFlags.None,
      IsWindowed = true,
      ModeDescription = new ModeDescription(
        this.renderControl1.ClientSize.Width,
        this.renderControl1.ClientSize.Height,
        new Rational(60, 1),
        Format.R8G8B8A8_UNorm),
      OutputHandle = this.renderControl1.Handle,
      SampleDescription = new SampleDescription(1, 0),
      SwapEffect = SwapEffect.Discard,
      Usage = Usage.RenderTargetOutput };

    SharpDX.Direct3D11.Device.CreateWithSwapChain(
      SharpDX.Direct3D.DriverType.Hardware,
      DeviceCreationFlags.Debug,
      _desc,
      out _gpu, out _sc);

    _renderTarget = Texture2D.FromSwapChain<Texture2D>(_sc, 0);
    _renderTargetView = new RenderTargetView(_gpu, _renderTarget);
    _depthBuffer = new Texture2D(_gpu, new Texture2DDescription() {
      Format = Format.D32_Float_S8X24_UInt,
      ArraySize = 1,
      MipLevels = 1,
      Width = renderControl1.ClientSize.Width,
      Height = renderControl1.ClientSize.Height,
      SampleDescription = new SampleDescription(1, 0),
      Usage = ResourceUsage.Default,
      BindFlags = BindFlags.DepthStencil,
      CpuAccessFlags = CpuAccessFlags.None,
      OptionFlags = ResourceOptionFlags.None, });
    _depthBufferView = new DepthStencilView(_gpu, _depthBuffer);
    _gpu.ImmediateContext.OutputMerger.SetTargets(_depthBufferView, _renderTargetView);

    _mtime = File.GetLastWriteTime(Path.Join(_dataDir, _sceneFileName));
    string sceneText = File.ReadAllText(Path.Join(_dataDir, _sceneFileName));
    _runningGraph = scene.GraphBuilder.Build(sceneText, _builtins);
    if (_runningGraph.root is null) {
      throw new Exception("did not find a layer node with id __main__"); }
    _runningGraph.Init(_gpu); }

  private
  void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
    _gpu?.Dispose();
    _sc?.Dispose();
    _renderTargetView?.Dispose();
    _renderTarget?.Dispose(); }

  private
  void MainForm_Paint(object sender, PaintEventArgs e) { }

  private
  void renderControl1_Paint(object sender, PaintEventArgs e) {
    if (_renderResized) {
      _renderResized = false;

      _renderTarget?.Dispose();
      _renderTargetView?.Dispose();
      _depthBufferView?.Dispose();
      _depthBuffer?.Dispose();

      _sc.ResizeBuffers(_desc.BufferCount,
                        renderControl1.ClientSize.Width,
                        renderControl1.ClientSize.Height,
                        Format.Unknown,
                        SwapChainFlags.None);

      _renderTarget = Texture2D.FromSwapChain<Texture2D>(_sc, 0);
      _renderTargetView = new RenderTargetView(_gpu, _renderTarget);
      _depthBuffer = new Texture2D(_gpu, new Texture2DDescription() {
        Format = Format.D32_Float_S8X24_UInt,
        ArraySize = 1,
        MipLevels = 1,
        Width = renderControl1.ClientSize.Width,
        Height = renderControl1.ClientSize.Height,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.DepthStencil,
        CpuAccessFlags = CpuAccessFlags.None,
        OptionFlags = ResourceOptionFlags.None, });
      _depthBufferView = new DepthStencilView(_gpu, _depthBuffer); }

    var dc = _gpu.ImmediateContext;
    dc.Rasterizer.SetViewport(0, 0, this.renderControl1.ClientSize.Width, this.renderControl1.ClientSize.Height, 0.0F, 1.0F);
    dc.OutputMerger.SetTargets(_depthBufferView, _renderTargetView);

    _systemValues.Upsert("T", _clock.ElapsedMilliseconds / 1000.0F);
    _systemValues.Upsert("canvasSize", new Vector2(renderControl1.ClientSize.Width, renderControl1.ClientSize.Height));
    _systemValues.Upsert("pixelAspect", new Vector2(1, 1));

    if (_depthBufferView is null) {
        Console.WriteLine("dbv is null!");
        throw new Exception("badness!"); }
    dc.ClearDepthStencilView(_depthBufferView, DepthStencilClearFlags.Depth, 1.0F, 0);

    if (_runningGraph.root is scene.ILayer rootLayer) {
      var tmp = rootLayer.GetColor();
      dc.ClearRenderTargetView(_renderTargetView, new RawColor4(tmp.X, tmp.Y, tmp.Z, 1.0F));
      rootLayer.Draw(dc); }

    _sc.Present(0, PresentFlags.None); }

  private
  void timer1_Tick(object sender, EventArgs e) {
    this.renderControl1.Invalidate(); }

  private
  void renderControl1_Resize(object sender, EventArgs e) {
    _renderResized = true; }

  private
  void quitToolStripMenuItem_Click(object sender, EventArgs e) {
    this.Close(); } }


}  // close package namespace
