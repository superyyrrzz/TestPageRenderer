namespace TestPageRenderer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using Microsoft.OpenPublishing.Build.PageRenderer;

    using Moq;
    using Newtonsoft.Json;
    using Ploeh.AutoFixture;

    class Program
    {
        private static void Main(string[] args)
        {
            // arrange
            var layout = "Conceptual";
            var themesRelativePath = Constants.ThemesFolderName;
            var layoutRelativePath = Path.Combine(themesRelativePath, $"{layout}.html.liquid");
            Fixture fixture = new Fixture();
            var rawPageDatas = new[]
            {
                new Dictionary<string, object>
                {
                    ["content"] = "contentValue1",
                    ["pageMetadata"] = "metadataValue1",
                    ["themesRelativePath"] = themesRelativePath,
                    ["rawMetadata"] = new Dictionary<string, object>
                    {
                        ["layout"] = layout
                    }
                },
                new Dictionary<string, object>
                {
                    ["content"] = "contentValue2",
                    ["pageMetadata"] = "metadataValue2",
                    ["themesRelativePath"] = themesRelativePath,
                    ["rawMetadata"] = new Dictionary<string, object>
                    {
                        ["layout"] = layout
                    }
                }
            };
            var templates = new[]
            {
                "[Template1] content: {{ content }}\r\nmetadata: {{ metadata }}",
                "[Template2] content: {{ content }}\r\nmetadata: {{ metadata }}"
            };
            var expectedResult = new[]
            {
                "[Template1] content: contentValue1\r\nmetadata: metadataValue1",
                "[Template2] content: contentValue2\r\nmetadata: metadataValue2"
            };

            for (var i = 0; i < rawPageDatas.Length; i++)
            {
                var rawPageDataString = JsonConvert.SerializeObject(rawPageDatas[i]);
                var relativePath = fixture.Create<string>();
                var mockContentProvider = new Mock<IContentProvider>();
                mockContentProvider.Setup(foo => foo.GetContentAsync(It.IsAny<string>()))
                    .Throws<FileNotFoundException>();
                mockContentProvider.Setup(foo => foo.GetContentAsync(relativePath)).ReturnsAsync(rawPageDataString);
                mockContentProvider.Setup(foo => foo.GetContentAsync(layoutRelativePath)).ReturnsAsync(templates[i]);

                // act
                var renderer = new PageRenderer(mockContentProvider.Object);
                var result = renderer.RenderAsync(relativePath).Result;
                Console.WriteLine(result);

                // assert
                Debug.Assert(expectedResult[i] == result);
            }
        }
    }
}
