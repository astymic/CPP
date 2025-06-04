using IRT.Classes;
using IRT.ConsoleOutput;

namespace IRT
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container<Product>();

            container.Add(new Product("Apple", 1.2m));
            container.Add(new Product("Banana", 0.8m));
            container.Add(new Product("Cherry", 2.5m));
            container.Add(new Product("Date", 3.0m));

            container.Add(new Product("Elderberry", 1.5m));
            container.Add(new Hotel("Elderberry", 1.5m, "zxc", 123, "asd", 2, 2));
            container.Add(new RealEstate("Elderberry", 1.5m, "zxc",123,"zxc"));
            container.Add(new Apartment("Elderberry", 1.5m, "asd", 123, "", 2, 2));

            // Serialize the container to a file
            string serializedFile = ContainerSerializer.SerializeContainer(container, "products");
            System.Console.WriteLine($"Container serialized to {serializedFile}");
            // Deserialize the container from the file
            //var deserializedContainer = ContainerSerializer.DeserializeContainer("products");
            //System.Console.WriteLine("Container deserialized from file:");
            //if (deserializedContainer is Container<Product> deserialized)
            //{
            //    foreach (var item in deserialized)
            //    {
            //        System.Console.WriteLine(item);
            //    }
            //}
            //else
            //{
            //    System.Console.WriteLine("Deserialized object is not of type Container<Product>");
            //}



            //var displayManager = new DisplayManager();
            //var dataFactory = new DataFactory();
            //var containerManager = new ContainerManager();
            //var propertyEditor = new PropertyEditor(displayManager, containerManager);

            //var operationHandlers = new OperationHandlers(
            //    containerManager,
            //    dataFactory,
            //    displayManager,
            //    propertyEditor
            //);

            //var consoleUI = new ConsoleUI(
            //    containerManager,
            //    operationHandlers,
            //    displayManager
            //);

            //consoleUI.Run();
        }
    }
}