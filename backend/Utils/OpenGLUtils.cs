using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace backend.Utils
{
    public class GlVersionInfo
    {
        public bool IsSupported { get; set; }
        public string Version { get; set; } = "Unknown";
        public string Vendor { get; set; } = "Unknown";
        public string Renderer { get; set; } = "Unknown";
        public string Error { get; set; }
    }

    public static class OpenGLUtils
    {
        public static GlVersionInfo GetGlInfo()
        {
            var info = new GlVersionInfo();

            IWindow window = null;
            GL gl = null;

            try
            {
                // 1. Configure a hidden, headless window
                var options = WindowOptions.Default;
                options.Size = new Vector2D<int>(100, 100); // Size doesn't matter
                options.Title = "Version Check";
                options.IsVisible = false; // Hidden
                options.VSync = false;
                options.ShouldSwapAutomatically = false;

                // 2. Create and Initialize
                window = Window.Create(options);
                window.Initialize();

                // 3. Create OpenGL Context
                gl = window.CreateOpenGL();

                // 4. Query Information
                info.Vendor = gl.GetStringS(StringName.Vendor);
                info.Renderer = gl.GetStringS(StringName.Renderer);
                info.Version = gl.GetStringS(StringName.Version);
                info.IsSupported = true;
            }
            catch (Exception ex)
            {
                info.IsSupported = false;
                info.Error = $"OpenGL Initialization Failed: {ex.Message}";
            }
            finally
            {
                // 5. Cleanup immediately
                gl?.Dispose();
                window?.Dispose();
            }

            return info;
        }
    }
}

