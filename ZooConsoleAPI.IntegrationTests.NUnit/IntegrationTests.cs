

using Microsoft.EntityFrameworkCore;
using NuGet.Frameworks;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using ZooConsoleAPI.Business;
using ZooConsoleAPI.Business.Contracts;
using ZooConsoleAPI.Data.Model;
using ZooConsoleAPI.DataAccess;

namespace ZooConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestAnimalDbContext dbContext;
        private IAnimalsManager animalsManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestAnimalDbContext();
            this.animalsManager = new AnimalsManager(new AnimalRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddAnimalAsync_ShouldAddNewAnimal()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9Q",
                Name = "Kaya",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = true,
            };

            await animalsManager.AddAsync(newAnimal);

            // Act
            var dbResult = await dbContext.Animals.FirstOrDefaultAsync(a => a.CatalogNumber == newAnimal.CatalogNumber);

            // Assert
            Assert.IsNotNull(dbResult);
            Assert.That(dbResult.CatalogNumber, Is.EqualTo(newAnimal.CatalogNumber));
            Assert.That(dbResult.Name, Is.EqualTo(newAnimal.Name));
            Assert.That(dbResult.Breed, Is.EqualTo(newAnimal.Breed));
            Assert.That(dbResult.Type, Is.EqualTo(newAnimal.Type));
            Assert.That(dbResult.Age, Is.EqualTo(newAnimal.Age));
            Assert.That(dbResult.Gender, Is.EqualTo(newAnimal.Gender));
            Assert.That(dbResult.IsHealthy, Is.EqualTo(newAnimal.IsHealthy));
        }

        //Negative test
        [Test]
        public async Task AddAnimalAsync_TryToAddAnimalWithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9", // length 11 (required length is 12 cahracters)
                Name = "Kaya",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = true,
            };

            // Act and Assert
            var exception = Assert.ThrowsAsync<ValidationException>(() => animalsManager.AddAsync(newAnimal));

            var result = await dbContext.Animals.FirstOrDefaultAsync(a => a.CatalogNumber == newAnimal.CatalogNumber);

            Assert.That(exception.Message, Is.EqualTo("Invalid animal!"));
            Assert.IsNull(result);
        }

        [Test]
        public async Task DeleteAnimalAsync_WithValidCatalogNumber_ShouldRemoveAnimalFromDb()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9Q",
                Name = "Rey",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Male",
                IsHealthy = true,
            };

            // Act
            await animalsManager.AddAsync(newAnimal);

            await animalsManager.DeleteAsync(newAnimal.CatalogNumber);

            var dbResult = await dbContext.Animals.FirstOrDefaultAsync(a => a.CatalogNumber == newAnimal.CatalogNumber);

            // Assert
            Assert.IsNull(dbResult);
        }

        [TestCase("")]
        [TestCase("            ")]
        [TestCase(null)]
        public async Task DeleteAnimalAsync_TryToDeleteWithNullOrWhiteSpaceCatalogNumber_ShouldThrowException(string invalidCatalogNumber)
        {
            // Arrange, Act, Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => animalsManager.DeleteAsync(invalidCatalogNumber));
            Assert.That(exception.Message, Is.EqualTo("Catalog number cannot be empty."));
        }

        [Test]
        public async Task GetAllAsync_WhenAnimalsExist_ShouldReturnAllAnimals()
        {
            // Arrange
            var firstAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9Q",
                Name = "Kaya",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = true,
            };

            var secondAnimal = new Animal()
            {
                CatalogNumber = "85SHTSRFPZ9R",
                Name = "Rey",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Male",
                IsHealthy = true,
            };

            await animalsManager.AddAsync(firstAnimal);
            await animalsManager.AddAsync(secondAnimal);

            // Act
            var allAnimals = await animalsManager.GetAllAsync();

            // Assert
            Assert.IsNotNull(allAnimals);
            Assert.That(allAnimals.Count, Is.EqualTo(2));

            // First Aminal
            Assert.That(allAnimals.First().CatalogNumber, Is.EqualTo(firstAnimal.CatalogNumber));
            Assert.That(allAnimals.First().Name, Is.EqualTo(firstAnimal.Name));
            Assert.That(allAnimals.First().Breed, Is.EqualTo(firstAnimal.Breed));
            Assert.That(allAnimals.First().Type, Is.EqualTo(firstAnimal.Type));
            Assert.That(allAnimals.First().Age, Is.EqualTo(firstAnimal.Age));
            Assert.That(allAnimals.First().Gender, Is.EqualTo(firstAnimal.Gender));
            Assert.That(allAnimals.First().IsHealthy, Is.EqualTo(firstAnimal.IsHealthy));

            // Second Animal(Last)
            Assert.That(allAnimals.Last().CatalogNumber, Is.EqualTo(secondAnimal.CatalogNumber));
            Assert.That(allAnimals.Last().Name, Is.EqualTo(secondAnimal.Name));
            Assert.That(allAnimals.Last().Breed, Is.EqualTo(secondAnimal.Breed));
            Assert.That(allAnimals.Last().Type, Is.EqualTo(secondAnimal.Type));
            Assert.That(allAnimals.Last().Age, Is.EqualTo(secondAnimal.Age));
            Assert.That(allAnimals.Last().Gender, Is.EqualTo(secondAnimal.Gender));
            Assert.That(allAnimals.Last().IsHealthy, Is.EqualTo(secondAnimal.IsHealthy));
        }

        [Test]
        public async Task GetAllAsync_WhenNoAnimalsExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange, Act, Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => animalsManager.GetAllAsync());
            Assert.That(exception.Message, Is.EqualTo("No animal found."));
        }

        [Test]
        public async Task SearchByTypeAsync_WithExistingType_ShouldReturnMatchingAnimals()
        {
            // Arrange
            var firstAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9Q",
                Name = "Kaya",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = true,
            };

            var secondAnimal = new Animal()
            {
                CatalogNumber = "85SHTSRFPZ9R",
                Name = "Angry bird",
                Breed = "Mini spitz",
                Type = "Bird",
                Age = 4,
                Gender = "Male",
                IsHealthy = false,
            };           

            await animalsManager.AddAsync(firstAnimal);
            await animalsManager.AddAsync(secondAnimal);

            // Act
            var allAnimals = await animalsManager.SearchByTypeAsync(firstAnimal.Type);

            // Assert
            Assert.IsNotNull(allAnimals);
            Assert.That(allAnimals.Count, Is.EqualTo(1));

            // First Aminal
            Assert.That(allAnimals.First().CatalogNumber, Is.EqualTo(firstAnimal.CatalogNumber));
            Assert.That(allAnimals.First().Name, Is.EqualTo(firstAnimal.Name));
            Assert.That(allAnimals.First().Breed, Is.EqualTo(firstAnimal.Breed));
            Assert.That(allAnimals.First().Type, Is.EqualTo(firstAnimal.Type));
            Assert.That(allAnimals.First().Age, Is.EqualTo(firstAnimal.Age));
            Assert.That(allAnimals.First().Gender, Is.EqualTo(firstAnimal.Gender));
            Assert.That(allAnimals.First().IsHealthy, Is.EqualTo(firstAnimal.IsHealthy));
        }

        [Test]
        public async Task SearchByTypeAsync_WithNonExistingType_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9Q",
                Name = "Kaya",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = true,
            };

            await animalsManager.AddAsync(newAnimal);

            // Act
            string noExistingType = "Bird";
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => animalsManager.SearchByTypeAsync(noExistingType));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("No animal found with the given type."));

        }

        [Test]
        public async Task GetSpecificAsync_WithValidCatalogNumber_ShouldReturnAnimal()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9Q",
                Name = "Kaya",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = true,
            };

            await animalsManager.AddAsync(newAnimal);

            // Act
            var result = await animalsManager.GetSpecificAsync(newAnimal.CatalogNumber);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.CatalogNumber, Is.EqualTo(newAnimal.CatalogNumber));
            Assert.That(result.Name, Is.EqualTo(newAnimal.Name));
            Assert.That(result.Breed, Is.EqualTo(newAnimal.Breed));
            Assert.That(result.Type, Is.EqualTo(newAnimal.Type));
            Assert.That(result.Age, Is.EqualTo(newAnimal.Age));
            Assert.That(result.Gender, Is.EqualTo(newAnimal.Gender));
            Assert.That(result.IsHealthy, Is.EqualTo(newAnimal.IsHealthy));
        }

        [TestCase("12B45A7891C")] // 11 symbols
        [TestCase("12B45AB89999C")] // 13 symbls 
        [TestCase("HH123GGJ0L1a")] // lower symbol
        [TestCase("D34A0OLHT56!")] // special symbol
        [TestCase("AAAAAAAAAAAA")] // only 12 letters
        [TestCase("134523674531")] // only 12 numbers
        [TestCase("123456ASDF 5")] // white space
        public async Task GetSpecificAsync_WithInvalidCatalogNumber_ShouldThrowKeyNotFoundException(string invalidCatalogNumber)
        {
            // Arrange,  Act
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => animalsManager.GetSpecificAsync(invalidCatalogNumber));

            // Assert
            Assert.That(exception.Message, Is.EqualTo($"No animal found with catalog number: {invalidCatalogNumber}"));
        }

        [Test]
        public async Task UpdateAsync_WithValidAnimal_ShouldUpdateAnimal()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9Q",
                Name = "Kaya",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = true,
            };

            await animalsManager.AddAsync(newAnimal);

            // Act
            newAnimal.Age = 3;

            await animalsManager.UpdateAsync(newAnimal);

            var result = await dbContext.Animals.FirstOrDefaultAsync(a => a.CatalogNumber == newAnimal.CatalogNumber);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.CatalogNumber, Is.EqualTo(newAnimal.CatalogNumber));
            Assert.That(result.Name, Is.EqualTo(newAnimal.Name));
            Assert.That(result.Breed, Is.EqualTo(newAnimal.Breed));
            Assert.That(result.Type, Is.EqualTo(newAnimal.Type));
            Assert.That(result.Age, Is.EqualTo(newAnimal.Age));
            Assert.That(result.Gender, Is.EqualTo(newAnimal.Gender));
            Assert.That(result.IsHealthy, Is.EqualTo(newAnimal.IsHealthy));   
        }

        [Test]
        public async Task UpdateAsync_WithInvalidAnimal_ShouldThrowValidationException()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "11HHTYRSDG9Q",
                Name = "Kaya",
                Breed = "Mini spitz",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = true,
            };

            await animalsManager.AddAsync(newAnimal);

            // Act
            newAnimal.Age = -6;

            var exception = Assert.ThrowsAsync<ValidationException>(() => animalsManager.UpdateAsync(newAnimal));
            Assert.That(exception.Message, Is.EqualTo("Invalid animal!"));
        }
    }
}

