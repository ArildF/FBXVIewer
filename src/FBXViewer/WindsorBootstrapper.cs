using System.Drawing;
using System.Windows.Media.Imaging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FBXViewer.OpenGL;
using FBXViewer.Wpf;

namespace FBXViewer
{
    public class WindsorBootstrapper
    {
        public static WindsorContainer Bootstrap()
        {
            var container = new WindsorContainer();

            container.Register(Classes.FromAssemblyInThisApplication(typeof(WindsorBootstrapper).Assembly)
                .BasedOn<INode>().LifestyleTransient());
            container.Register(Component.For<MainWindow>().ImplementedBy<MainWindow>());
            container.Register(Component.For<MainWindowViewModel>());
            container.Register(Component.For<TreeNodeViewModel>().LifestyleTransient());
            container.Register(Component.For<ModelPreview>().LifestyleSingleton());
            container.Register(Component.For<IScene>().ImplementedBy<OpenGLScene>().LifestyleSingleton());
            // container.Register(Component.For<IScene>().ImplementedBy<WpfScene>().LifestyleSingleton());
            container.Register(Component.For<MeshLoader>().LifestyleSingleton());
            container.Register(Component.For(typeof(TextureProvider<>)).LifestyleSingleton());
            container.Register(Component.For<TextureSearcher>().LifestyleSingleton());
            container.Register(Component.For<MaterialProvider>().LifestyleSingleton());
            container.Register(Component.For<Coroutines>().LifestyleSingleton());
            container.Register(Component.For<SceneContext>().LifestyleSingleton());

            container.Register(Component.For<ITextureLoader<BitmapSource>>()
                .ImplementedBy<BitmapSourceTextureLoader>());
            container.Register(Component.For<ITextureLoader<Bitmap>>()
                .ImplementedBy<BitmapTextureLoader>());
            
            container.Kernel.AddFacility<TypedFactoryFacility>();

            return container;
        }
    }
}