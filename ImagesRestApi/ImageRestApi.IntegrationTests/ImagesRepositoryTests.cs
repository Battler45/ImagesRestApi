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
using ImagesRestApi.Profiles;
using ImagesRestApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace ImageRestApi.IntegrationTests
{
    public class ImagesRepositoryTests
    {
        private const string TestImagesDatabaseName = "ImagesTest";
        //очевидно, этот тест излишен, но он написан по фану.
        //this test is excessive, but it was written for lulz
        [Fact]
        public async Task GetImageBy_Id_Got()
        {
            //Arrange
            var fakeImagesEntities = CreateRandomFakeImages().Item1;
            var options = new DbContextOptionsBuilder<ImagesContext>()
                .UseInMemoryDatabase(TestImagesDatabaseName)
                .Options;
            await using (var context = new ImagesContext(options))
            {
                await context.AddRangeAsync(fakeImagesEntities);
                await context.SaveChangesAsync();
            }
            var loggerMock = new Mock<ILogger<ImagesRepository>>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ImagesProfile());
            });
            var mapper = mapperConfig.CreateMapper();
            var expected = fakeImagesEntities[new Random().Next(0, fakeImagesEntities.Count)];

            //Act
            ImageDTO resultFakeEtitiy;
            await using (var context = new ImagesContext(options))
            {
                var repository = new ImagesRepository(context, loggerMock.Object, mapper);
                resultFakeEtitiy = await repository.GetImageAsync(expected.Id);
            }

            //Assert
            await using (var context = new ImagesContext(options))
            {
                expected.Should().BeEquivalentTo(resultFakeEtitiy);
                context.Database.EnsureDeleted();
            }
        }

        [Theory]
        [InlineData(50, 20)]
        [InlineData(10, 5)]
        public async Task GetImageBy_Ids_Got(int fakeRandomEntitiesCount, int expectedEntitiesCount)
        {
            //Arrange
            var fakeImagesEntities = CreateRandomFakeImages(fakeRandomEntitiesCount).Item1;
            var expected = fakeImagesEntities.Take(expectedEntitiesCount);
            var options = new DbContextOptionsBuilder<ImagesContext>()
                .UseInMemoryDatabase(TestImagesDatabaseName)
                .Options;
            await using (var context = new ImagesContext(options))
            {
                await context.AddRangeAsync(fakeImagesEntities);
                await context.SaveChangesAsync();
            }
            var loggerMock = new Mock<ILogger<ImagesRepository>>();
            var mapper = new MapperConfiguration(cfg =>cfg.AddProfile(new ImagesProfile())).CreateMapper();

            //Act
            List<ImageDTO> resultFakeEtities;
            await using (var context = new ImagesContext(options))
            {
                var repository = new ImagesRepository(context, loggerMock.Object, mapper);
                resultFakeEtities = await repository.GetImagesAsync(expected.Select(e => e.Id));
            }

            //Assert
            await using (var context = new ImagesContext(options))
            {
                expected.Should().BeEquivalentTo(resultFakeEtities);
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
