#version 430 core
in vec2 aPosition;
in vec2 aTexCoord;

uniform vec2 anchor;
uniform vec2 posOffset;
uniform vec2 posScale;
uniform vec2 sizeOffset;  // remove
uniform vec2 sizeScale;  // remove

uniform vec2 screenSize;

uniform int atlasColumns;
uniform int atlasRows;
uniform int atlasWidth;
uniform int atlasHeight;
uniform int characterInclusionOffset;

struct Glyph
{
    vec2 origin;
    vec2 size;
    vec2 bearing;
    float advance;
    float uselessSpace;
};

layout(std430, binding = 0) buffer ssbo0
{
    Glyph metrics[];
};

layout(std430, binding = 1) buffer ssbo1
{
    int text[];
};

out vec2 texCoord;

float GetGlobalOrigin(int instance)
{
    float origin = 0.0;

    for (int i = 0; i < instance; i++)
    {
        Glyph gl = metrics[text[i]+characterInclusionOffset];
        origin += gl.advance - gl.bearing.x;
    }

    return origin;
}

void main()
{
    //vec2 sizing = sizeScale + (sizeOffset/screenSize);
    int glyphID = text[gl_InstanceID] + characterInclusionOffset;

    Glyph glyph = metrics[glyphID];

    vec2 sizing = glyph.size*vec2(1,2)/screenSize;

    vec2 anchorOffset = sizing * anchor;

    float globalOrigin = GetGlobalOrigin(gl_InstanceID);

    vec2 positioning = posScale + ((posOffset+vec2(globalOrigin, 0) - glyph.bearing)/screenSize)
                     - ((vec2(1,1) - sizing) / 2)
                     - anchorOffset;

    positioning = vec2(positioning.x, -positioning.y);

    gl_Position = vec4(aPosition * sizing + positioning, 0.0, 0.5);

    vec2 glyphAtlasPosition = vec2(glyphID % atlasColumns, glyphID / atlasColumns);

    float cellWidth = 1.0 / atlasColumns;
    float cellHeight = 1.0 / atlasRows;

    float width = (glyph.size.x) / atlasWidth;
    float height = cellHeight;

    vec2 offset = vec2(glyphAtlasPosition.x * cellWidth, glyphAtlasPosition.y * cellHeight);
    offset += vec2((glyph.origin.x) / atlasWidth, (cellHeight * atlasHeight - glyph.origin.y) / atlasHeight);

    float uvs[8];
    uvs[0] = offset.x;
    uvs[1] = offset.y + height;
    uvs[2] = offset.x;
    uvs[3] = offset.y;
    uvs[4] = offset.x + width;
    uvs[5] = offset.y;
    uvs[6] = offset.x + width;
    uvs[7] = offset.y + height;

    texCoord = vec2(uvs[gl_VertexID*2], 1-uvs[gl_VertexID*2 + 1]);
}