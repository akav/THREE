using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using System;

namespace WPFDemo.Controls
{
    public class GLRenderControl : GLControl
    {
        public EventHandler<EventArgs> OnInitGL;


        public GLRenderControl() : base()
        {
            InitGLContext();
        }

        private void GLRenderControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            throw new NotImplementedException();
        }

        /*public GLRenderControl(GraphicsMode mode) : base(mode)
        {
            InitGLContext();
        }*/

        private void InitGLContext()
        {
#if NET6_0_OR_GREATER
            this.Profile = OpenTK.Windowing.Common.ContextProfile.Compatability;
#endif

            this.MakeCurrent();
            //this.VSync = true;
            if (OnInitGL != null)
            {
                OnInitGL(this, new EventArgs());
            }


        }
        ///*public GLRenderControl(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags) : base(mode, major, minor, flags)
        //{
        //    InitGLContext();
        //}*/

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.IsHandleCreated)
            {
                SetViewport(0, 0, this.Size.Width, this.Size.Height);
                //if(scene!=null)
                //    scene.Resize(this.Size.Width,this.Size.Height);
            }
        }
        public void SetViewport(int x, int y, int width, int height)
        {

            GL.Viewport(x, y, width, height);
        }


        //public void Clean()
        //{
        //    //GL.ClearColor(System.Drawing.Color.FromArgb(this.ClearAlpha, this.ClearColor));
        //    GL.ClearBuffer(ClearBuffer.Color, 0, new float[4] { 0, 0, 0, 1 });
        //}


    }
}
