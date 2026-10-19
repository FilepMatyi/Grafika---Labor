using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
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

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. szeminárium - háromszög";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

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

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float[] vertexArray = new float[27 * 4 * 3];

            float[] colorArray = new float[27 * 4 * 4];

            uint[] indexArray = new uint[27 * 6];

            int vertexPos = 0;
            int colorPos = 0;
            int indexPos = 0;

            float gap = 0.015f;


            // 1. elülső oldal - piros 
            // A = (-0.5, -0.5)
            // B = ( 0.2, -0.5)
            // C = ( 0.2,  0.2)
            // D = (-0.5,  0.2

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float s0 = col / 3.0f + gap;
                    float s1 = (col + 1) / 3.0f - gap;

                    float t0 = row / 3.0f + gap;
                    float t1 = (row + 1) / 3.0f - gap;

                    float x0 = -0.5f + 0.7f * s0;
                    float x1 = -0.5f + 0.7f * s1;

                    float y0 = -0.5f + 0.7f * t0;
                    float y1 = -0.5f + 0.7f * t1;

                    uint firstVertex = (uint)(vertexPos / 3);

                    // bal alsó
                    vertexArray[vertexPos++] = x0;
                    vertexArray[vertexPos++] = y0;
                    vertexArray[vertexPos++] = 0.0f;

                    // jobb alsó
                    vertexArray[vertexPos++] = x1;
                    vertexArray[vertexPos++] = y0;
                    vertexArray[vertexPos++] = 0.0f;

                    // jobb felső
                    vertexArray[vertexPos++] = x1;
                    vertexArray[vertexPos++] = y1;
                    vertexArray[vertexPos++] = 0.0f;

                    // bal felső
                    vertexArray[vertexPos++] = x0;
                    vertexArray[vertexPos++] = y1;
                    vertexArray[vertexPos++] = 0.0f;

                    // piros - mind a 4 vertex
                    for (int i = 0; i < 4; i++)
                    {
                        colorArray[colorPos++] = 1.0f;
                        colorArray[colorPos++] = 0.0f;
                        colorArray[colorPos++] = 0.0f;
                        colorArray[colorPos++] = 1.0f;
                    }

                    // két háromszög
                    indexArray[indexPos++] = firstVertex;
                    indexArray[indexPos++] = firstVertex + 1;
                    indexArray[indexPos++] = firstVertex + 2;

                    indexArray[indexPos++] = firstVertex;
                    indexArray[indexPos++] = firstVertex + 2;
                    indexArray[indexPos++] = firstVertex + 3;
                }
            }


            
            // 2. felső oldal - zöld
            // D = (-0.5, 0.2)
            // C = ( 0.2, 0.2)
            // G = ( 0.5, 0.45)
            // H = (-0.2, 0.45)
            
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float s0 = col / 3.0f + gap;
                    float s1 = (col + 1) / 3.0f - gap;

                    float t0 = row / 3.0f + gap;
                    float t1 = (row + 1) / 3.0f - gap;

                    float x0 = -0.5f + 0.7f * s0 + 0.3f * t0;
                    float y0 = 0.2f + 0.25f * t0;

                    float x1 = -0.5f + 0.7f * s1 + 0.3f * t0;
                    float y1 = 0.2f + 0.25f * t0;

                    float x2 = -0.5f + 0.7f * s1 + 0.3f * t1;
                    float y2 = 0.2f + 0.25f * t1;

                    float x3 = -0.5f + 0.7f * s0 + 0.3f * t1;
                    float y3 = 0.2f + 0.25f * t1;

                    uint firstVertex = (uint)(vertexPos / 3);

                    vertexArray[vertexPos++] = x0;
                    vertexArray[vertexPos++] = y0;
                    vertexArray[vertexPos++] = 0.0f;

                    vertexArray[vertexPos++] = x1;
                    vertexArray[vertexPos++] = y1;
                    vertexArray[vertexPos++] = 0.0f;

                    vertexArray[vertexPos++] = x2;
                    vertexArray[vertexPos++] = y2;
                    vertexArray[vertexPos++] = 0.0f;

                    vertexArray[vertexPos++] = x3;
                    vertexArray[vertexPos++] = y3;
                    vertexArray[vertexPos++] = 0.0f;

                    // zöld
                    for (int i = 0; i < 4; i++)
                    {
                        colorArray[colorPos++] = 0.0f;
                        colorArray[colorPos++] = 1.0f;
                        colorArray[colorPos++] = 0.0f;
                        colorArray[colorPos++] = 1.0f;
                    }

                    indexArray[indexPos++] = firstVertex;
                    indexArray[indexPos++] = firstVertex + 1;
                    indexArray[indexPos++] = firstVertex + 2;

                    indexArray[indexPos++] = firstVertex;
                    indexArray[indexPos++] = firstVertex + 2;
                    indexArray[indexPos++] = firstVertex + 3;
                }
            }


            
            // 3. jobb oldal - kék
            
            // B = (0.2, -0.5)
            // C = (0.2,  0.2)
            // F = (0.5, -0.25)
            // G = (0.5,  0.45)
            
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float s0 = col / 3.0f + gap;
                    float s1 = (col + 1) / 3.0f - gap;

                    float t0 = row / 3.0f + gap;
                    float t1 = (row + 1) / 3.0f - gap;

                    float x0 = 0.2f + 0.3f * s0;
                    float y0 = -0.5f + 0.25f * s0 + 0.7f * t0;

                    float x1 = 0.2f + 0.3f * s1;
                    float y1 = -0.5f + 0.25f * s1 + 0.7f * t0;

                    float x2 = 0.2f + 0.3f * s1;
                    float y2 = -0.5f + 0.25f * s1 + 0.7f * t1;

                    float x3 = 0.2f + 0.3f * s0;
                    float y3 = -0.5f + 0.25f * s0 + 0.7f * t1;

                    uint firstVertex = (uint)(vertexPos / 3);

                    vertexArray[vertexPos++] = x0;
                    vertexArray[vertexPos++] = y0;
                    vertexArray[vertexPos++] = 0.0f;

                    vertexArray[vertexPos++] = x1;
                    vertexArray[vertexPos++] = y1;
                    vertexArray[vertexPos++] = 0.0f;

                    vertexArray[vertexPos++] = x2;
                    vertexArray[vertexPos++] = y2;
                    vertexArray[vertexPos++] = 0.0f;

                    vertexArray[vertexPos++] = x3;
                    vertexArray[vertexPos++] = y3;
                    vertexArray[vertexPos++] = 0.0f;

                    // kék
                    for (int i = 0; i < 4; i++)
                    {
                        colorArray[colorPos++] = 0.0f;
                        colorArray[colorPos++] = 0.0f;
                        colorArray[colorPos++] = 1.0f;
                        colorArray[colorPos++] = 1.0f;
                    }

                    indexArray[indexPos++] = firstVertex;
                    indexArray[indexPos++] = firstVertex + 1;
                    indexArray[indexPos++] = firstVertex + 2;

                    indexArray[indexPos++] = firstVertex;
                    indexArray[indexPos++] = firstVertex + 2;
                    indexArray[indexPos++] = firstVertex + 3;
                }
            }

            uint vertices = Gl.GenBuffer();

            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);

            Gl.BufferData(GLEnum.ArrayBuffer,(ReadOnlySpan<float>)vertexArray.AsSpan(),GLEnum.StaticDraw);

            Gl.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false,0,null);

            Gl.EnableVertexAttribArray(0);


            uint colors = Gl.GenBuffer();

            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);

            Gl.BufferData(GLEnum.ArrayBuffer,(ReadOnlySpan<float>)colorArray.AsSpan(),GLEnum.StaticDraw);

            Gl.VertexAttribPointer(1,4,VertexAttribPointerType.Float,false,0,null);

            Gl.EnableVertexAttribArray(1);


            uint indices = Gl.GenBuffer();

            Gl.BindBuffer(GLEnum.ElementArrayBuffer,indices);

            Gl.BufferData(GLEnum.ElementArrayBuffer,(ReadOnlySpan<uint>)indexArray.AsSpan(),GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);

            Gl.DrawElements(GLEnum.Triangles,(uint)indexArray.Length,GLEnum.UnsignedInt,null);

            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);

            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}