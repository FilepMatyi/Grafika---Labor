using Silk.NET.OpenGL;

namespace GrafikaSzeminarium
{
    internal class ModelObjectDescriptor:IDisposable
    {
        private bool disposedValue;

        private bool ownsGeometry = true;

        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }

        private GL Gl;

        public unsafe static ModelObjectDescriptor CreateRubikCubie(GL Gl, CubeGeometryDescriptor geometry, float[] colorArray)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            Gl.BindBuffer(GLEnum.ArrayBuffer, geometry.Vertices);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.BindBuffer(GLEnum.ElementArrayBuffer, geometry.Indices);

            Gl.BindVertexArray(0);

            return new ModelObjectDescriptor() { Vao = vao, Vertices = geometry.Vertices, Colors = colors, Indices = geometry.Indices, IndexArrayLength = geometry.IndexArrayLength, Gl = Gl, ownsGeometry = false };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                if (ownsGeometry)
                {
                    Gl.DeleteBuffer(Vertices);
                    Gl.DeleteBuffer(Indices);
                }
                Gl.DeleteBuffer(Colors);
                Gl.DeleteVertexArray(Vao);

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
