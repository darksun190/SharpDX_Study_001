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
        int indices_count;
        static Vector4 ColorBlue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        static Vector4 ColorRed = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        static Vector4 ColorGreen = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        static Vector4 ColorWhite = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        static Vector4 ColorBlack = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        static Vector4 ColorYellow = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        static Vector4 ColorCyan = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);

        struct Data
        {
            public Matrix world;
            public Matrix viewProj;
            public Vector4 factor;
        }
        Device device;
        SwapChain swapChain;
        Color4 color;
        DeviceContext context;
        RenderTargetView renderView;
        Matrix projection;
        Matrix view;
        Matrix world;

        Matrix WVP;
        Buffer contantBuffer;
        Camera camera;
        public Form1()
        {
            InitializeComponent();
            initDevice();
            camera = new Camera();
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
                  2,8,6
                  ,2,6,0
           };
            List<int> indices_list = new List<int>();
            List<Vector4> vertices_list = new List<Vector4>();
            {
                //craete a grid
                float width = 50.0f;
                float depth = 50.0f;
                int m = 10;
                int n = 10;
                int vertexCount = m * n;
                int faceCount = (m - 1) * (n - 1) * 2;

                //
                // Create the vertices.
                //

                float halfWidth = 0.5f * width;
                float halfDepth = 0.5f * depth;

                float dx = width / (n - 1);
                float dz = depth / (m - 1);

                float du = 1.0f / (n - 1);
                float dv = 1.0f / (m - 1);

                for (int i = 0; i < m; ++i)
                {
                    float z = halfDepth - i * dz;
                    for (int j = 0; j < n; ++j)
                    {
                        float x = -halfWidth + j * dx;

                        vertices_list.Add(
                            new Vector4(x, 0.3f * (z * Convert.ToSingle(Math.Sin(0.1f * x)) + x * Convert.ToSingle(Math.Cos(0.1f * z))), z, 1.0f)
                            );

                    }
                }
                //
                // Create the indices.
                //


                // Iterate over each quad and compute indices.
                for (int i = 0; i < m - 1; ++i)
                {
                    for (int j = 0; j < n - 1; ++j)
                    {
                        indices_list.Add( i * n + j);
                        indices_list.Add(i * n + j + 1);
                        indices_list.Add((i + 1) * n + j);

                        indices_list.Add((i + 1) * n + j);
                        indices_list.Add(i * n + j + 1);
                        indices_list.Add((i + 1) * n + j + 1);

                    }
                }
            }
            indices_count = indices_list.Count;
            var vertices = Buffer.Create(device, BindFlags.VertexBuffer, vertices_list.ToArray());
            var indices = Buffer.Create(device, BindFlags.IndexBuffer, indices_list.ToArray());

            // Ignore all windows events
            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);

            // Create Constant Buffer
            contantBuffer = new Buffer(device, Utilities.SizeOf<Data>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("HLSL.hlsl", "VS", "vs_5_0");
            var vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("HLSL.hlsl", "PS", "ps_4_0");
            var pixelShader = new PixelShader(device, pixelShaderByteCode);

            var hullShaderByteCode = ShaderBytecode.CompileFromFile("HLSL.hlsl", "HS", "hs_5_0");
            var HullShader = new HullShader(device, hullShaderByteCode);

            var domainShaderByteCode = ShaderBytecode.CompileFromFile("HLSL.hlsl", "DS", "ds_5_0");
            var DomainShader = new DomainShader(device, domainShaderByteCode);

            var geometryShaderByteCode = ShaderBytecode.CompileFromFile("HLSL.hlsl", "GS", "gs_5_0");
            var GeometryShader = new GeometryShader(device, geometryShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            // Layout from VertexShader input signature
            var layout = new InputLayout(device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0)
                    });





            // Prepare All the stages

            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PatchListWith3ControlPoints;

            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, Utilities.SizeOf<Vector4>(), 0));
            context.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);
            context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            context.GeometryShader.Set(GeometryShader);
            context.DomainShader.Set(DomainShader);
            context.HullShader.Set(HullShader);
            // Prepare All the stages
            context.Rasterizer.SetViewport(new Viewport(0, 0, ClientSize.Width, ClientSize.Height, 0.5f, 1.0f));
            var rdsc = RasterizerStateDescription.Default();
            rdsc.FillMode = FillMode.Solid;
            context.Rasterizer.State = new RasterizerState(device, rdsc);
            context.OutputMerger.SetTargets(renderView);


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            camera.LookAt(new Vector3(0, 80, 0), new Vector3(), Vector3.UnitZ);

            float ratio = (float)ClientRectangle.Width / (float)ClientRectangle.Height;
            //projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 100);
            camera.SetLens(3.15f / 3.0f, ratio, 1, 1000);
            camera.UpdateViewMatrix();
            world = Matrix.Identity;
            RenderLoop.Run(this, Draw3D);
        }

        private void Draw3D()
        {
            context.ClearRenderTargetView(renderView, color);
            camera.UpdateViewMatrix();

            var WVP = new Data()
            {
                world = world,
                viewProj = camera.ViewProj,
                factor = new Vector4(2)
            };

            context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.PixelShader.SetConstantBuffer(0, contantBuffer);
            context.GeometryShader.SetConstantBuffer(0, contantBuffer);
            context.HullShader.SetConstantBuffer(0, contantBuffer);
            context.DomainShader.SetConstantBuffer(0, contantBuffer);

            context.UpdateSubresource(ref WVP, contantBuffer);

            context.DrawIndexed(indices_count, 0, 0);
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
        System.Drawing.Point posLast
        {
            get;
            set;
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

            posLast = e.Location;
            this.MouseMove += Form1_MouseMove;

        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var deltaX = (e.X - posLast.X) / 100.0f;
                var deltaY = (e.Y - posLast.Y) / 100.0f;
                posLast = e.Location;
                world = world * Matrix.RotationAxis(Vector3.Normalize(camera.Right), -deltaY);
                world = world * Matrix.RotationAxis(Vector3.Normalize(camera.Up), -deltaX);

                //camera.Pitch(-deltaY);
                //camera.RotateY(deltaX);
            }
            else if (e.Button == MouseButtons.Left)
            {
                var deltaX = (e.X - posLast.X) / 100.0f;
                var deltaY = (e.Y - posLast.Y) / 100.0f;
                //System.Diagnostics.Debug.WriteLine(string.Format(
                //    "delta Value: {0}\t{1}", deltaX, deltaY));
                camera.Strafe(-deltaX);
                camera.Walk(deltaY);
                posLast = e.Location;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            this.MouseMove -= Form1_MouseMove;
        }
    }
}
