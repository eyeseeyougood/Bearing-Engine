#version 430 core

in vec2 aPosition;
in vec2 aTexCoord;

uniform vec2 anchor;
uniform vec2 posOffset;
uniform vec2 posScale;

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
        Glyph gl = metrics[text[i] + characterInclusionOffset];

        origin += gl.advance;
    }

    return origin;
}

void main()
{
    int glyphID = text[gl_InstanceID] + characterInclusionOffset;

    Glyph glyph = metrics[glyphID];

    vec2 sizing = glyph.size/screenSize;

    vec2 anchorOffset = sizing * anchor;

    float globalOrigin = GetGlobalOrigin(gl_InstanceID);

    vec2 positioning = posScale + ((posOffset + vec2(globalOrigin, 0.0) - glyph.bearing)/screenSize)
        -((vec2(1.0) - sizing) / 2.0)
        -anchorOffset;

    positioning = vec2(positioning.x, -positioning.y);

    gl_Position = vec4(aPosition * sizing + positioning, 0.0, 0.5);

    vec2 cellPosition = vec2(glyphID % atlasColumns, glyphID / atlasColumns);
    vec2 cellSize = vec2(1.0 / atlasColumns, 1.0 / atlasRows);
    vec2 cellUV = vec2(cellPosition.x * cellSize.x, cellPosition.y * cellSize.y);

    vec2 glyphSize = vec2(glyph.size.x / atlasWidth, glyph.size.y / atlasHeight);
    vec2 glyphUVOffset = vec2(glyph.origin.x / atlasWidth, glyph.origin.y / atlasHeight);

    vec2 offset = cellUV + glyphUVOffset;

    vec2 uv;

    if (gl_VertexID == 0)
    {
        uv = vec2(offset.x, offset.y + glyphSize.y);
    }
    else if (gl_VertexID == 1)
    {
        uv = vec2(offset.x, offset.y);
    }
    else if (gl_VertexID == 2)
    {
        uv = vec2(offset.x + glyphSize.x, offset.y);
    }
    else
    {
        uv = vec2(offset.x + glyphSize.x, offset.y + glyphSize.y);
    }

    texCoord = vec2(uv.x, 1.0 - uv.y);
}