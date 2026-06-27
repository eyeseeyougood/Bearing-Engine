#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColour;

struct Tile {
    int x;
    int y;

    int index;
};

#define NO_TILES 300

uniform Tile tiles[NO_TILES];
uniform int numTiles;

uniform vec2 tileSizeMultiplier;
uniform vec2 sliceSize;
uniform vec2 sheetSize;

uniform sampler2D texture0;

vec4 IndexToSampleCoords(int index)
{
    int tileCountX = int(sheetSize.x / sliceSize.x);
    return vec4(mod(index, tileCountX) * sliceSize.x, int(index / tileCountX) * sliceSize.y, sliceSize.x, sliceSize.y);
}

vec2 SampleCoordsToTexCoords(vec4 sampleCoords, vec2 fragPercentageDepth)
{
    return vec2((sampleCoords.x + sampleCoords.z * fragPercentageDepth.x), (sampleCoords.y + sampleCoords.w - sampleCoords.w * fragPercentageDepth.y)) / sheetSize;
}

void main()
{
    vec4 finalCol = mainColour;

    bool isInTile = false;
    for (int i = 0; i < numTiles; i++)
    {
        Tile currentTile = tiles[i];
        if (gl_FragCoord.x >= currentTile.x * sliceSize.x && gl_FragCoord.x < (currentTile.x + 1) * sliceSize.x)
        {
            if (gl_FragCoord.y >= currentTile.y * sliceSize.y && gl_FragCoord.y < (currentTile.y + 1) * sliceSize.y)
            {
                vec2 fragPercentageDepth = vec2(gl_FragCoord.x / sliceSize.x - currentTile.x, gl_FragCoord.y / sliceSize.y - currentTile.y);

                vec4 sampleCoords = IndexToSampleCoords(currentTile.index);
                vec2 tileTexCoord = SampleCoordsToTexCoords(sampleCoords, fragPercentageDepth);
                finalCol = mainColour * texture(texture0, tileTexCoord);
                //finalCol = vec4(tileTexCoord,0,1);
                isInTile = true;
            }
        }
    }

    if (!isInTile)
        discard;

    FragColor = finalCol;
}