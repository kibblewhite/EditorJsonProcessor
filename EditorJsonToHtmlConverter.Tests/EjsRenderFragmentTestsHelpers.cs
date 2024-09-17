internal static class EjsRenderFragmentTestsHelpers
{

    public static readonly string EditorJsonEmpty = """
        {
          "time": 1707325917682,
          "blocks": [],
          "version": "2.28.2"
        }
        """;

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
              "id": "zOGADPv7kl",
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
              "id": "zOGFDv7kl",
              "type" : "delimiter",
              "data" : {}
            },
            {
              "id": "zOUIDPv7kl",
              "type": "warning",
              "data": {
                "title": "Note:",
                "message": "Avoid using this method just for lulz. It can be very dangerous opposite your daily fun stuff."
              }
            },
            {
              "id": "zOJKDPv7kl",
              "type" : "delimiter",
              "data" : {}
            },
            {
              "id": "zUKNDPv7kl",
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

    public static readonly string EditorHtmlValue = """
        !!! <i>He</i>y<b>lo, W</b>orld! <a href="https://google.com">Link</a> !!!<h1 id="KgrM3aNM-n"><mark class="cdx-marker"><a href="http://google.com">Heylo</a></mark></h1><p id="NaTtEbbeRT">Heylo World</p><h3 id="KgrM3aNM-n">Second header</h3><ol id="QdqCFpKBAm"><li>A: One<ul><li>B: Two<ul></ul></li></ul></li><li>A: Three<ul><li>B: Four<ul><li>C: Five<ul></ul></li></ul></li><li>B: Six<ul></ul></li><li>B: Seven<ul></ul></li></ul></li></ol><blockquote id="m-onbmz6BZ" class="text-left">Ohhh interesting...<footer>by Me!</footer></blockquote><p id="ZatOSzA754">dsf<i>sfa</i><b>sfasdfs</b>dffasd</p><ul id="SWrBNzvp6A"><li>dwdw</li><li>wedwed</li><li>wedw</li></ul><ul id="yD5ZHUxF1N" style="list-style-type: none;"><li><input type="checkbox" disabled="true" />Check List Item One</li><li><input type="checkbox" disabled="true" checked="checked" />Check List Item Two</li><li><input type="checkbox" disabled="true" />Check List Item Three</li></ul><hr id="J5I_aD9c8j" /><table id="J-7FqxXppm"><thead><tr><th>Header 1</th><th>Header 2</th><th>Header 3</th></tr></thead><tbody><tr><td>qwerty</td><td>as<b>dfg</b>h</td><td>zxc<mark class="cdx-marker">vbn</mark></td></tr><tr><td>AAA</td><td><a href="https://google.com/">BBB</a></td><td><code class="inline-code">CCC</code></td></tr></tbody></table><table id="zOGIbPv7kl"><tbody><tr><td>A1</td><td>B1</td></tr><tr><td>A2</td><td>B2</td></tr></tbody></table><img id="zOGADPv7kl" src="https://www.tesla.com/tesla_theme/assets/img/_vehicle_redesign/roadster_and_semi/roadster/hero.jpg" alt="Roadster // tesla.com" style="border: 1px solid #ddd; width: 100%;" /><p style="text-align: center;">Roadster // tesla.com</p><hr id="zOGFDv7kl" /><div id="zOUIDPv7kl"><strong>Note:</strong><p>Avoid using this method just for lulz. It can be very dangerous opposite your daily fun stuff.</p></div><hr id="zOJKDPv7kl" /><div id="zUKNDPv7kl"><iframe src="https://coub.com/embed/1czcdf" width="100%" height="320" style="margin: 0 auto;" frameborder="0" allowfullscreen="true"></iframe></div><p>My Life</p>
        """;

    public static readonly string ExpectedEditorHtmlValue = """
        !!! <i>He</i>y<b>lo, W</b>orld! <a href="https://google.com">Link</a> !!!<h1 id="KgrM3aNM-n"><mark class="cdx-marker"><a href="http://google.com">Heylo</a></mark></h1><p id="NaTtEbbeRT">Heylo World</p><h3 id="KgrM3aNM-n">Second header</h3><ol id="QdqCFpKBAm"><li>A: One<ul><li>B: Two<ul></ul></li></ul></li><li>A: Three<ul><li>B: Four<ul><li>C: Five<ul></ul></li></ul></li><li>B: Six<ul></ul></li><li>B: Seven<ul></ul></li></ul></li></ol><blockquote id="m-onbmz6BZ" class="text-left">Ohhh interesting...<footer>by Me!</footer></blockquote><p id="ZatOSzA754">dsf<i>sfa</i><b>sfasdfs</b>dffasd</p><ul id="SWrBNzvp6A"><li>dwdw</li><li>wedwed</li><li>wedw</li></ul><ul id="yD5ZHUxF1N" role="group" style="list-style-type: none;"><li aria-hidden="true"><input type="checkbox" disabled="true" />Check List Item One</li><li aria-hidden="true"><input type="checkbox" disabled="true" checked="checked" />Check List Item Two</li><li aria-hidden="true"><input type="checkbox" disabled="true" />Check List Item Three</li></ul><hr id="J5I_aD9c8j" /><table id="J-7FqxXppm"><thead><tr><th>Header 1</th><th>Header 2</th><th>Header 3</th></tr></thead><tbody><tr><td>qwerty</td><td>as<b>dfg</b>h</td><td>zxc<mark class="cdx-marker">vbn</mark></td></tr><tr><td>AAA</td><td><a href="https://google.com/">BBB</a></td><td><code class="inline-code">CCC</code></td></tr></tbody></table><table id="zOGIbPv7kl"><tbody><tr><td>A1</td><td>B1</td></tr><tr><td>A2</td><td>B2</td></tr></tbody></table><img id="zOGADPv7kl" src="https://www.tesla.com/tesla_theme/assets/img/_vehicle_redesign/roadster_and_semi/roadster/hero.jpg" alt="Roadster // tesla.com" style="border: 1px solid #ddd; width: 100%;" /><p style="text-align: center;">Roadster // tesla.com</p><hr id="zOGFDv7kl" /><div id="zOUIDPv7kl"><strong>Note:</strong><p>Avoid using this method just for lulz. It can be very dangerous opposite your daily fun stuff.</p></div><hr id="zOJKDPv7kl" /><div id="zUKNDPv7kl"><iframe src="https://coub.com/embed/1czcdf" width="100%" height="320" style="margin: 0 auto;" frameborder="0" allowfullscreen="true"></iframe></div><p>My Life</p>
        """;

    public static readonly string ExpectedEditorHtmlWithStylingsValue = """
        !!! <i>He</i>y<b>lo, W</b>orld! <a href="https://google.com">Link</a> !!!<h1 id="KgrM3aNM-n" class="specific-style-1"><mark class="cdx-marker"><a href="http://google.com">Heylo</a></mark></h1><p id="NaTtEbbeRT" class="specific-style-p">Heylo World</p><h3 id="KgrM3aNM-n" class="general-style-3">Second header</h3><ol id="QdqCFpKBAm" class="list-group list-group-flush"><li class="list-group-item">A: One<ul><li>B: Two<ul></ul></li></ul></li><li class="list-group-item">A: Three<ul><li>B: Four<ul><li>C: Five<ul></ul></li></ul></li><li>B: Six<ul></ul></li><li>B: Seven<ul></ul></li></ul></li></ol><blockquote id="m-onbmz6BZ" class="text-left blockquote">Ohhh interesting...<footer class="blockquote-footer">by Me!</footer></blockquote><p id="ZatOSzA754" class="specific-style-p">dsf<i>sfa</i><b>sfasdfs</b>dffasd</p><ul id="SWrBNzvp6A" class="list-group list-group-flush"><li class="list-group-item">dwdw</li><li class="list-group-item">wedwed</li><li class="list-group-item">wedw</li></ul><ul id="yD5ZHUxF1N" role="group" style="list-style-type: none;" class="list-group"><li aria-hidden="true" class="list-group-item"><input type="checkbox" disabled="true" />Check List Item One</li><li aria-hidden="true" class="list-group-item"><input type="checkbox" disabled="true" checked="checked" />Check List Item Two</li><li aria-hidden="true" class="list-group-item"><input type="checkbox" disabled="true" />Check List Item Three</li></ul><hr id="J5I_aD9c8j" class="delimiter" /><table id="J-7FqxXppm" class="table table-hover"><thead><tr><th>Header 1</th><th>Header 2</th><th>Header 3</th></tr></thead><tbody><tr><td>qwerty</td><td>as<b>dfg</b>h</td><td>zxc<mark class="cdx-marker">vbn</mark></td></tr><tr><td>AAA</td><td><a href="https://google.com/">BBB</a></td><td><code class="inline-code">CCC</code></td></tr></tbody></table><table id="zOGIbPv7kl" class="table table-hover"><tbody><tr><td>A1</td><td>B1</td></tr><tr><td>A2</td><td>B2</td></tr></tbody></table><img id="zOGADPv7kl" class="img-fluid" src="https://www.tesla.com/tesla_theme/assets/img/_vehicle_redesign/roadster_and_semi/roadster/hero.jpg" alt="Roadster // tesla.com" style="border: 1px solid #ddd; width: 100%;" /><p style="text-align: center;">Roadster // tesla.com</p><hr id="zOGFDv7kl" class="delimiter" /><div id="zOUIDPv7kl" class="warning"><strong>Note:</strong><p>Avoid using this method just for lulz. It can be very dangerous opposite your daily fun stuff.</p></div><hr id="zOJKDPv7kl" class="delimiter" /><div id="zUKNDPv7kl"><iframe src="https://coub.com/embed/1czcdf" width="100%" height="320" style="margin: 0 auto;" frameborder="0" allowfullscreen="true"></iframe></div><p>My Life</p>
        """;

    public static readonly string StylingMap = """
        [
            {
                "type": "header",
                "level": 1,
                "style": "specific-style-1"
            },
            {
                "type": "header",
                "level": 2,
                "style": "general-style-2"
            },
            {
                "type": "header",
                "level": 3,
                "style": "general-style-3"
            },
            {
                "type": "header",
                "level": 4,
                "style": "general-style-4"
            },
            {
                "type": "header",
                "level": 5,
                "style": "general-style-5"
            },
            {
                "type": "paragraph",
                "style": "specific-style-p"
            },
            {
                "type": "list",
                "style": "list-group list-group-flush",
                "item-style": "list-group-item"
            },
            {
                "type": "checklist",
                "style": "list-group",
                "item-style": "list-group-item"
            },
            {
                "type": "quote",
                "style": "blockquote",
                "footer-style": "blockquote-footer"
            },
            {
                "type": "table",
                "style": "table table-hover"
            },
            {
                "type": "image",
                "style": "img-fluid"
            },
            {
                "type": "delimiter",
                "style": "delimiter"
            },
            {
                "type": "warning",
                "style": "warning"
            },
            {
                "type": "text",
                "style": "text"
            }
        ]
        """;
}