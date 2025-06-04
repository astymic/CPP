using IRT.Classes;
using IRT.ConsoleOutput;

namespace IRT
{
    class Program
    {
        static void Main(string[] args)
        {
            var displayManager = new DisplayManager();
            var dataFactory = new DataFactory();
            var containerManager = new ContainerManager();
            var propertyEditor = new PropertyEditor(displayManager, containerManager);

            var operationHandlers = new OperationHandlers(
                containerManager,
                dataFactory,
                displayManager,
                propertyEditor
            );

            var consoleUI = new ConsoleUI(
                containerManager,
                operationHandlers,
                displayManager
            );

            consoleUI.Run();
        }
    }
}