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
            windowOptions.Size =
                new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }


        // OpenGL hibák ellenőrzése
        private static void CheckError(string hely)
        {
            GLEnum error = Gl.GetError();

            if (error != GLEnum.NoError)
            {
                Console.WriteLine("Hiba itt: " + hely + " -> " + error);
            }
        }


        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);


            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);

            Gl.GetShader(
                vshader,
                ShaderParameterName.CompileStatus,
                out int vStatus);

            // kitörlünk ; jelet a vertex shaderből,
            // akkor a shader nem fordul le
            // ezt a CompileStatus és a GetShaderInfoLogban látjuk
            if (vStatus != (int)GLEnum.True)
            {
                Console.WriteLine(
                    "Vertex shader hiba: "
                    + Gl.GetShaderInfoLog(vshader));
            }


            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            Gl.GetShader(
                fshader,
                ShaderParameterName.CompileStatus,
                out int fStatus);

            if (fStatus != (int)GLEnum.True)
            {
                Console.WriteLine(
                    "Fragment shader hiba: "
                    + Gl.GetShaderInfoLog(fshader));
            }


            program = Gl.CreateProgram();

            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);

            Gl.LinkProgram(program);

            Gl.GetProgram(
                program,
                GLEnum.LinkStatus,
                out int status);

            // ha a két shader nem illik össze,
            // vagy valamelyik nincs jól lefordítva,
            // akkor a program hibás lesz
            if (status == 0)
            {
                Console.WriteLine(
                    "Shader link hiba: "
                    + Gl.GetProgramInfoLog(program));
            }


            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);

            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            CheckError("shaderek létrehozása");
        }


        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
        }


        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);


            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            CheckError("VAO létrehozása");


            float[] vertexArray = new float[]
            {
                -0.5f, -0.5f, 0.0f,
                 0.5f, -0.5f, 0.0f,
                 0.0f,  0.5f, 0.0f,
                 1.0f,  1.0f, 0.0f
            };

            // ha egy koordinátát kitörlünk a tömbből,
            // a koordináták elcsúsznak és rossz lesz a rajz


            float[] colorArray = new float[]
            {
                1.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f
            };

            // ha egy szín floatot kitörlünk,
            // a színek elcsúszhatnak

            uint[] indexArray = new uint[]
            {
                0, 1, 2,
                2, 1, 3
            };

            // ha egy háromszög egyik csúcspontját kivesszük a tömbből,
            // a háromszögek rosszul lesznek rajzolva


            uint vertices = Gl.GenBuffer();

            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);

            Gl.BufferData(
                GLEnum.ArrayBuffer,
                (ReadOnlySpan<float>)vertexArray.AsSpan(),
                GLEnum.StaticDraw);

            Gl.VertexAttribPointer(
                0,
                3,
                VertexAttribPointerType.Float,
                false,
                0,
                null);

            Gl.EnableVertexAttribArray(0);

            // a BindBuffer sort törlése,
            // a BufferData nem tudja melyik bufferbe tegye az adatot
            // GetError hiba

            CheckError("vertex buffer");

            uint colors = Gl.GenBuffer();

            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);

            Gl.BufferData(
                GLEnum.ArrayBuffer,
                (ReadOnlySpan<float>)colorArray.AsSpan(),
                GLEnum.StaticDraw);

            Gl.VertexAttribPointer(
                1,
                4,
                VertexAttribPointerType.Float,
                false,
                0,
                null);

            Gl.EnableVertexAttribArray(1); 

            // EnableVertexAttribArray(1) helyett
            // EnableVertexAttribArray(2)-t írunk
            // a színek hibásak lehetnek

            CheckError("color buffer");


            // index buffer

            uint indices = Gl.GenBuffer();

            Gl.BindBuffer(
                GLEnum.ElementArrayBuffer,
                indices);

            Gl.BufferData(
                GLEnum.ElementArrayBuffer,
                (ReadOnlySpan<uint>)indexArray.AsSpan(),
                GLEnum.StaticDraw);

            CheckError("index buffer");


            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);

            // ha a LinkProgram kimarad,
            // akkor a programot nem lehet rendesen használni
            // a GetError hibát jelezhet a UseProgram után

            CheckError("UseProgram");

            Gl.DrawElements(
                GLEnum.Triangles,
                (uint)indexArray.Length,
                GLEnum.UnsignedInt,
                null);

            CheckError("DrawElements");

            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);

            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}