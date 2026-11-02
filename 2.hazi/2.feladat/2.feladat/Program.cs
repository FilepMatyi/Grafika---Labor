using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static CubeGeometryDescriptor cubeGeometry;


        private static List<ModelObjectDescriptor> rubikCubies = new List<ModelObjectDescriptor>();


        private static RubikCubeArrangementModel rubikCubeArrangementModel = new RubikCubeArrangementModel();

        private static CameraDescriptor camera = new CameraDescriptor();

        private const string ModelMatrixVariableName = "uModel";
        private const string FaceRotationMatrixVariableName = "uFaceRotation";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uFaceRotation;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;

        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uFaceRotation*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;

		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Grafika szeminárium - Rubik kocka";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            foreach (var cubie in rubikCubies)
            {
                cubie.Dispose();
            }
            cubeGeometry.Dispose();
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl.ClearColor(System.Drawing.Color.White);

            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

            cubeGeometry = CubeGeometryDescriptor.Create(Gl);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        float[] colorArray = RubikCubeArrangementModel.GetCubieColorArray(x, y, z);
                        rubikCubies.Add(ModelObjectDescriptor.CreateRubikCubie(Gl, cubeGeometry, colorArray));
                        rubikCubeArrangementModel.AddCubie(x, y, z);
                    }
                }
            }

            Gl.UseProgram(program);
            float aspectRatio = (float)graphicWindow.Size.X / graphicWindow.Size.Y;
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 4), aspectRatio, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Up:
                    camera.MoveForward();
                    break;
                case Key.Down:
                    camera.MoveBackward();
                    break;
                case Key.Left:
                    camera.MoveUp();
                    break;
                case Key.Right:
                    camera.MoveDown();
                    break;
                case Key.A:
                    camera.TurnLeft();
                    break;
                case Key.D:
                    camera.TurnRight();
                    break;
                case Key.W:
                    camera.TiltUp();
                    break;
                case Key.S:
                    camera.TiltDown();
                    break;
                case Key.Space:
                    rubikCubeArrangementModel.StartTopLayerRotation(1);
                    break;
                case Key.Backspace:
                    rubikCubeArrangementModel.StartTopLayerRotation(-1);
                    break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {

            rubikCubeArrangementModel.AdvanceTime(deltaTime);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var topLayerRotation = rubikCubeArrangementModel.TopLayerRotationMatrix;

            for (int i = 0; i < rubikCubies.Count; i++)
            {
                SetMatrix(rubikCubeArrangementModel.CubieModelMatrices[i], ModelMatrixVariableName);

                var faceRotation = rubikCubeArrangementModel.IsOnTopLayer(i) ? topLayerRotation : Matrix4X4<float>.Identity;
                SetMatrix(faceRotation, FaceRotationMatrixVariableName);

                DrawModelObject(rubikCubies[i]);
            }
        }

        private static unsafe void DrawModelObject(ModelObjectDescriptor modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}
