using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EditorJsonToHtmlConverter.Tests;

[TestClass]
public class EjsRenderFragmentTests : Bunit.TestContext
{
    private ILogger<EjsRenderFragment> _logger = default!;

    [TestInitialize]
    public void TestInitialise()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        ILoggerFactory factory = serviceProvider.GetRequiredService<ILoggerFactory>();

        _logger = factory.CreateLogger<EjsRenderFragment>();
    }

    [TestMethod]
    public void BasicTest()
    {
        // Arrange
        string styling_map = "[]";
        _ = _logger;

        // "does not resolve to a public property on the component" <- Google returnes zero results on this, so for now now unit testing until the produce matures a bit more.
        IRenderedComponent<EjsRenderFragment> cut = RenderComponent<EjsRenderFragment>(parameters => parameters
            .Add(p => p.Value, EditorJsonValue)
            .Add(p => p.StylingMap, styling_map));

        // Act
        string rendered_html = cut.Markup;

        // Assert
        // Add appropriate assertions to verify the HTML output
        // For example:
        // cut.Find("p").MarkupMatches("<p>Hello World</p>");
    }

    public static readonly string EditorJsonValue = """
        {
          "time": 1707325917682,
          "blocks": [
            {"id": "uM3Adn6C9n", "data": {"text": "!!! <i>He</i>y<b>lo, W</b>orld! <a href=\"https://google.com\">Link</a> !!!", "wrap": "title"}, "type": "text"},
            {
              "id": "KgrM3aNM-n",
              "type": "header",
              "data": {
                "text": "<mark class=\"cdx-marker\"><a href=\"http://google.com\">Heylo</a></mark>",
                "level": 1
              }
            },
            {
              "id": "NaTtEbbeRT",
              "type": "paragraph",
              "data": {
                "text": "Heylo World"
              }
            },
            {
              "id": "KgrM3aNM-n",
              "type": "header",
              "data": {
                "text": "Second header",
                "level": 3
              }
            },
            {
              "id": "QdqCFpKBAm",
              "type": "list",
              "data": {
                "style": "ordered",
                "items": [
                  {
                    "content": "A: One",
                    "items": [
                      {
                        "content": "B: Two",
                        "items": []
                      }
                    ]
                  },
                  {
                    "content": "A: Three",
                    "items": [
                      {
                        "content": "B: Four",
                        "items": [
                          {
                            "content": "C: Five",
                            "items": []
                          }
                        ]
                      },
                      {
                        "content": "B: Six",
                        "items": []
                      },
                      {
                        "content": "B: Seven",
                        "items": []
                      }
                    ]
                  }
                ]
              }
            },
            {
              "id": "m-onbmz6BZ",
              "type": "quote",
              "data": {
                "text": "Ohhh interesting...",
                "caption": "by Me!",
                "alignment": "left"
              }
            },
            {
              "id": "ZatOSzA754",
              "type": "paragraph",
              "data": {
                "text": "dsf<i>sfa</i><b>sfasdfs</b>dffasd"
              }
            },
            {
              "id": "SWrBNzvp6A",
              "type": "list",
              "data": {
                "style": "unordered",
                "items": [
                  {
                    "content": "dwdw",
                    "items": []
                  },
                  {
                    "content": "wedwed",
                    "items": []
                  },
                  {
                    "content": "wedw",
                    "items": []
                  }
                ]
              }
            },
        
            {
              "id": "yD5ZHUxF1N",
              "type": "checklist",
              "data": {
                "items": [
                  {
                    "text": "Check List Item One",
                    "checked": false
                  },
                  {
                    "text": "Check List Item Two",
                    "checked": true
                  },
                  {
                    "text": "Check List Item Three",
                    "checked": false
                  }
                ]
              }
            },
            {
              "id": "J5I_aD9c8j",
              "type": "delimiter",
              "data": {}
            },
            {
              "id": "J-7FqxXppm",
              "type": "table",
              "data": {
                "withHeadings": true,
                "content": [
                  [
                    "Header 1",
                    "Header 2",
                    "Header 3"
                  ],
                  [
                    "qwerty",
                    "as<b>dfg</b>h",
                    "zxc<mark class=\"cdx-marker\">vbn</mark>"
                  ],
                  [
                    "AAA",
                    "<a href=\"https://google.com/\">BBB</a>",
                    "<code class=\"inline-code\">CCC</code>"
                  ]
                ]
              }
            },
            {
              "id": "zOGIbPv7kl",
              "type": "table",
              "data": {
                "withHeadings": false,
                "content": [
                  [
                    "A1",
                    "B1"
                  ],
                  [
                    "A2",
                    "B2"
                  ]
                ]
              }
            },
            {
              "type": "image",
              "data": {
                "url": "https://www.tesla.com/tesla_theme/assets/img/_vehicle_redesign/roadster_and_semi/roadster/hero.jpg",
                "caption": "Roadster // tesla.com",
                "withBorder": true,
                "withBackground": false,
                "stretched": true
              }
            },
            {
              "type" : "delimiter",
              "data" : {}
            },
            {
              "type": "warning",
              "data": {
                "title": "Note:",
                "message": "Avoid using this method just for lulz. It can be very dangerous opposite your daily fun stuff."
              }
            },
            {
              "type" : "delimiter",
              "data" : {}
            },
            {
              "type" : "embed",
              "data" : {
                "service" : "coub",
                "source" : "https://coub.com/view/1czcdf",
                "embed" : "https://coub.com/embed/1czcdf",
                "width" : 580,
                "height" : 320,
                "caption" : "My Life"
              }
            }
          ],
          "version": "2.28.2"
        }
        """;
}