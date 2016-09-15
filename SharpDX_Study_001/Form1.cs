using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Windows;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;


namespace SharpDX_Study_001
{
    public partial class Form1 : RenderForm
    {
        Device device;
        SwapChain swapChain;
        Color4 color;
        DeviceContext context;
        RenderTargetView renderView;
        public Form1()
        {
            InitializeComponent();
            initDevice();
        }

        private void initDevice()
        {
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(ClientSize.Width, ClientSize.Height,
                                                                new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, new FeatureLevel[] { FeatureLevel.Level_11_0 }, desc, out device, out swapChain);
            //System.Diagnostics.Debug.Assert(device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, 4) > 0);
            context = device.ImmediateContext;
            Texture2D backBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0);
            renderView = new RenderTargetView(device, backBuffer);

            int[] indices_array = new int[]
           {
                0,1,2,0,2,3,
                4,6,5,4,7,6,
                8,9,10,8,10,11,
                12,14,13,12,15,14,
                16,18,17,16,19,18,
                20,21,22,20,22,23
           };
            var indices = Buffer.Create(device, BindFlags.IndexBuffer, indices_array);

            var vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                                           {
                                      new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
                                  });
            // Ignore all windows events
            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);
           
            // Create Constant Buffer
            var contantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("shader.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shader.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            // Layout from VertexShader input signature
            var layout = new InputLayout(device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                    });

            // Prepare All the stages

            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>() * 2, 0));
            context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            // Prepare All the stages
            context.Rasterizer.SetViewport(new Viewport(0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f));
            context.OutputMerger.SetTargets(renderView);


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RenderLoop.Run(this, Draw3D);
        }

        private void Draw3D()
        {
            context.ClearRenderTargetView(renderView, color);
            context.Draw(3, 0);
            swapChain.Present(0, 0);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.D1:

                    color = Color.CornflowerBlue;
                    break;
                case System.Windows.Forms.Keys.D2:
                    color = Color.Red;
                    break;
                case System.Windows.Forms.Keys.D3:
                    color = Color.Blue;
                    break;
                case System.Windows.Forms.Keys.D4:
                    color = Color.Orange;
                    break;
                case System.Windows.Forms.Keys.D5:
                    color = Color.Yellow;
                    break;
                case System.Windows.Forms.Keys.D6:
                    color = Color.Olive;
                    break;
                case System.Windows.Forms.Keys.D7:
                    color = Color.Orchid;
                    break;
                case System.Windows.Forms.Keys.D8:
                    color = Color.Black;
                    break;
            }
        }
    }
}
