using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task GetByIdAsync_ReturnsSeededStudent()
        {
            var repo = new InMemoryStudentRepository();
            repo.Seed(new Student(1, "Juan Dela Cruz"));

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Juan Dela Cruz", result!.Name);
        }
    }
}
