using Polls.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TestEntity : IIdentifiable
{
    public Guid Id { get; }
    public string Name { get; set; }

    public TestEntity(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    [JsonConstructor]
    public TestEntity(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}


[TestClass]
public class GenericCrudServiceAsyncTests
{
    [TestMethod]
    public async Task CreateAsync_Should_Add_Element_Successfully()
    {
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity = new TestEntity("Test");

        bool createResult = await service.CreateAsync(entity);
        var readResult = await service.ReadAsync(entity.Id);

        Assert.IsTrue(createResult, "CreateAsync повинен повернути true для нового елемента.");
        Assert.IsNotNull(readResult, "Елемент повинен бути знайдений після створення.");
        Assert.AreEqual(entity, readResult, "Елементи повинні співпадати після занесення до сервісу та зчитування.");
    }

    [TestMethod]
    public async Task CreateAsync_Should_Fail_When_Element_Exists()
    {
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity = new TestEntity("Test");
        await service.CreateAsync(entity);

        bool result = await service.CreateAsync(entity);

        Assert.IsFalse(result, "CreateAsync повинен повернути false якщо елемент вже існує.");
    }

    [TestMethod]
    public async Task ReadAsync_Should_Return_Element_When_Exists()
    {
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity = new TestEntity("Test");
        await service.CreateAsync(entity);

        var result = await service.ReadAsync(entity.Id);

        Assert.AreEqual(entity.Id, result.Id, "Елементи повинні співпадати після занесення до сервісу та зчитування.");
    }

    [TestMethod]
    public async Task ReadAllAsync_Should_Return_All_Elements()
    {
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity1 = new TestEntity("Test 1");
        var entity2 = new TestEntity("Test 2");
        await service.CreateAsync(entity1);
        await service.CreateAsync(entity2);

        var results = await service.ReadAllAsync();

        Assert.AreEqual(2, results.Count(), "Кількість повернутих елементів повинна бути 2.");
    }

    [TestMethod]
    public async Task ReadAllAsync_Should_Return_Paged_Elements()
    {
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity1 = new TestEntity("Test 1");
        var entity2 = new TestEntity("Test 2");
        await service.CreateAsync(entity1);
        await service.CreateAsync(entity2);

        var results = await service.ReadAllAsync(1, 1);

        Assert.AreEqual(1, results.Count(), "Кількість повернутих елементів повинна бути 1.");
    }

    [TestMethod]
    public async Task UpdateAsync_Should_Modify_Element_Successfully()
    {
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity = new TestEntity("Test 1");
        await service.CreateAsync(entity);
        var newName = "Test 2";

        entity.Name = newName;
        bool updateResult = await service.UpdateAsync(entity);
        var updatedEntity = await service.ReadAsync(entity.Id);

        Assert.IsTrue(updateResult, "UpdateAsync повинен повернути true.");
        Assert.AreEqual(newName, updatedEntity.Name, "Ім'я елемента повинно було оновитися.");
    }

    [TestMethod]
    public async Task UpdateAsync_Should_Throw_Exception_When_Not_Exists()
    {
        // Arrange
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity = new TestEntity("Test");

        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => service.UpdateAsync(entity),
            "Повинен видати помилку KeyNotFoundException."
        );
    }
    
    [TestMethod]
    public async Task RemoveAsync_Should_Delete_Element_Successfully()
    {
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity = new TestEntity("Test");
        await service.CreateAsync(entity);

        bool result = await service.RemoveAsync(entity);

        Assert.IsTrue(result, "RemoveAsync повинен повернути true для існуючого елемента.");
    }
    
    [TestMethod]
    public async Task SaveAsync_Should_Create_File_With_Correct_Content()
    {
        var service = new GenericCrudServiceAsync<TestEntity>();
        var entity1 = new TestEntity("Test 1");
        var entity2 = new TestEntity("Test 2");
        await service.CreateAsync(entity1);
        await service.CreateAsync(entity2);

        await service.SaveAsync();

        Assert.IsTrue(File.Exists("polls.json"), "Файл повинен бути створений.");

        string json = await File.ReadAllTextAsync("polls.json");
        var content = JsonSerializer.Deserialize<Dictionary<Guid, TestEntity>>(json);

        Assert.IsNotNull(content, "Десерелізований вміст не повинен бути null.");
        Assert.AreEqual(2, content.Count, "Вміст повинен складатися з двох елементів.");
        Assert.IsTrue(content.ContainsKey(entity1.Id), "Вміст повинен містити перший елемент.");
        Assert.IsTrue(content.ContainsKey(entity2.Id), "Вміст повинен містити другий елемент.");
    }
}
