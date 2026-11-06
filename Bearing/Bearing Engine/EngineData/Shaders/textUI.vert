#version 430 core
in vec2 aPosition;
in vec2 aTexCoord;

uniform vec2 anchor;
uniform vec2 posOffset;
uniform vec2 posScale;
uniform vec2 sizeOffset;
uniform vec2 sizeScale;
uniform vec2 paddingOffset;
uniform vec2 paddingScale;
uniform vec2 alignment;
uniform int maxCharHeight;

uniform vec2 screenSize;

uniform int fitToSize;

out vec2 texCoord;

struct Glyph
{
    vec2 atlasPosition;
    vec2 atlasPercSize;
    vec2 atlasPixelSize;
    vec2 bearing;
    vec3 spacing;
    float advance;
};

layout(binding = 0, std430) readonly buffer ssbo0 {
    Glyph metrics[];
};

layout(binding = 1, std430) readonly buffer ssbo1 {
    int textUnicodes[];
};

layout(binding = 2, std430) readonly buffer ssbo2 {
    int kernings[];
};

int GetPosition()
{
    float pos = 0;

    int kern = 0;

    for (int i = 0; i < gl_InstanceID; i++)
    {
        Glyph gData = metrics[textUnicodes[i]];

        if (i > 0)
            kern = kernings[i-1];

        pos += gData.advance + kern;
    }

    pos += metrics[textUnicodes[gl_InstanceID]].bearing.x;

    return int(pos);
}

int GetTextWidth()
{
    float width = 0;
    int kern = 0;
    int length = textUnicodes.length();
    for (int i = 0; i < length-1; i++)
    {
        Glyph gData = metrics[textUnicodes[i]];

        if (i > 0)
            kern = kernings[i-1];

        width += gData.advance + kern;
    }

    Glyph gData = metrics[textUnicodes[length-1]];

    kern = kernings[length-2];

    width += gData.atlasPixelSize.x + kern;

    return int(width);
}

void main()
{
    int glyphUnicode = textUnicodes[gl_InstanceID];

    vec2 rectPixelSize = sizeOffset + sizeScale * screenSize;
    
    float fittingMult = 1;
    if (fitToSize == 1)
    {
        vec2 adjustedRect = rectPixelSize - paddingOffset - paddingScale * rectPixelSize;

        fittingMult = adjustedRect.y / maxCharHeight;

        float w = GetTextWidth();
        if (w * fittingMult > adjustedRect.x)
        {
            fittingMult = adjustedRect.x / w;
        }
    }

    vec2 textScreenSize = vec2(GetTextWidth(), maxCharHeight) * fittingMult;
    vec2 emptySpace = rectPixelSize - textScreenSize;

    vec2 alignmentOffset = emptySpace * alignment;

    vec2 sizing = metrics[glyphUnicode].atlasPixelSize * fittingMult/screenSize;

    vec2 anchorOffset = sizing * anchor;

    vec2 positioning = posScale + ((posOffset+vec2(GetPosition()*fittingMult + alignmentOffset.x,
                                    -metrics[textUnicodes[gl_InstanceID]].bearing.y * fittingMult + maxCharHeight*fittingMult + alignmentOffset.y))/screenSize)
                     - ((vec2(1,1) - sizing) / 2)
                     - anchorOffset;

    positioning.y = -positioning.y;

    gl_Position = vec4(aPosition * sizing + positioning, 0.0, 0.5);

    vec2 tex = vec2(0,0);
    int vid = gl_VertexID;
    if (vid == 3)
        tex = metrics[glyphUnicode].atlasPosition;
    else if (vid == 0)
        tex = metrics[glyphUnicode].atlasPosition + vec2(0,1) * metrics[glyphUnicode].atlasPercSize;
    else if (vid == 1)
        tex = metrics[glyphUnicode].atlasPosition + vec2(1,1) * metrics[glyphUnicode].atlasPercSize;
    else if (vid == 2)
        tex = metrics[glyphUnicode].atlasPosition + vec2(1,0) * metrics[glyphUnicode].atlasPercSize;


    texCoord = tex;
}