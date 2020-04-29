using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

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
            
            container.Kernel.AddFacility<TypedFactoryFacility>();

            return container;
        }
    }
}