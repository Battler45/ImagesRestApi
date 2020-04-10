using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using ImagesRestApi.Databases.Images;
using ImagesRestApi.Databases.Images.Entities;
using ImagesRestApi.DTO;
using ImagesRestApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ImageRestApi.UnitTests
{
    
    public class ImagesRepositoryTests   
    {
        private const string TestImagesDatabaseName = "ImagesTest";
        //очевидно, этот тест излишен, но он написан по фану.
        //this test is excessive, but it was written for lulz
        [Fact]
        public async Task SaveImagesBy_ImagesDTO_Saved()
        {
            //Arrange
            var (fakeImagesEntities, expectedImagesDto) = CreateRandomFakeImages();
            var options = new DbContextOptionsBuilder<ImagesContext>()
                .UseInMemoryDatabase(TestImagesDatabaseName)
                .Options;
            var mapperMock = new Mock<IMapper>();
                mapperMock.Setup(m => m.Map<IEnumerable<Image>>(It.IsAny<IEnumerable<ImageDTO>>()))
                    .Returns(fakeImagesEntities);

            //Act
            await using (var context = new ImagesContext(options))
            {
                var repository = new ImagesRepository(context, mapperMock.Object);
                await repository.SaveImages(expectedImagesDto);
            }

            //Assert
            await using (var context = new ImagesContext(options))
            {
                expectedImagesDto.Should().BeEquivalentTo(context.Images.ToList());
                context.Database.EnsureDeleted();
            }
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]//this test is excessive, but it was written for lulz
        public async Task DeleteImageBy_ExistingId_IsContainsId(bool isExistingId, bool isContainsId)
        {
            //Arrange
            var fakeImagesEntities = CreateRandomFakeImages().Item1;
            var deletedRandomFakeEntityId = isExistingId ? fakeImagesEntities[new Random().Next(0, fakeImagesEntities.Count)].Id : Guid.NewGuid();
            var options = new DbContextOptionsBuilder<ImagesContext>()
                .UseInMemoryDatabase(TestImagesDatabaseName)
                .Options;
            await using (var context = new ImagesContext(options))
            {
                await context.AddRangeAsync(fakeImagesEntities);
                await context.SaveChangesAsync();  
            }
            var mapperMock = new Mock<IMapper>();

            //Act
            await using (var context = new ImagesContext(options))
            {
                var repository = new ImagesRepository(context, mapperMock.Object);
                await repository.DeleteImage(deletedRandomFakeEntityId);
            }

            //Assert
            await using (var context = new ImagesContext(options))
            {
                var isRepositoryContainsId = context.Images.Any(i => i.Id == deletedRandomFakeEntityId);
                Assert.Equal(isContainsId, isRepositoryContainsId);
                context.Database.EnsureDeleted();
            }
        }

        [Theory]
        [InlineData(50, 20)]
        [InlineData(10, 5)]
        public async Task DeleteImagesBy_ExistingIds_IsContainsId(int fakeRandomEntitiesCount, int deletedFakeRandomEntitiesCount)
        {
            //Arrange
            var fakeImagesEntities = CreateRandomFakeImages(fakeRandomEntitiesCount).Item1;
            var deletedRandomFakeEntities = fakeImagesEntities.Take(deletedFakeRandomEntitiesCount);
            var exceptedStoredEntities = fakeImagesEntities.Except(deletedRandomFakeEntities);
            var options = new DbContextOptionsBuilder<ImagesContext>()
                .UseInMemoryDatabase(TestImagesDatabaseName)
                .Options;

            await using (var context = new ImagesContext(options))
            {
                await context.AddRangeAsync(fakeImagesEntities);
                await context.SaveChangesAsync();
            }
            var mapperMock = new Mock<IMapper>();

            //Act
            await using (var context = new ImagesContext(options))
            {
                var repository = new ImagesRepository(context, mapperMock.Object);
                await repository.DeleteImages(deletedRandomFakeEntities.Select(e => e.Id));
            }

            //Assert
            await using (var context = new ImagesContext(options))
            {
                exceptedStoredEntities.Should().BeEquivalentTo(context.Images.ToList());
                context.Database.EnsureDeleted();
            }
        }

        private (List<Image>, List<ImageDTO>) CreateRandomFakeImages(int fakeImagesCount = 2)
        {
            var fakeImagesEntities = new List<Image>(fakeImagesCount);
            var fakeImagesDTOs = new List<ImageDTO>(fakeImagesCount);
            for (var i = 0; i < fakeImagesCount; ++i)
            {
                var fakeImageId = Guid.NewGuid();
                var fakeImagePath = Path.GetRandomFileName(); 
                fakeImagesEntities.Add(new Image()
                {
                    Id = fakeImageId,
                    Path = fakeImagePath
                });
                fakeImagesDTOs.Add(new ImageDTO()
                {
                    Id = fakeImageId,
                    Path = fakeImagePath
                });
            }

            return (fakeImagesEntities, fakeImagesDTOs);
        }
    }
}
