#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColour;

uniform vec2 caretPos;
uniform vec2 caretSize;

uniform vec2 texSize;

uniform sampler2D texture0;

void main()
{
    vec4 finalCol = mainColour * texture(texture0, texCoord);

    vec2 pixCoord = texCoord * texSize;

    if (pixCoord.x > caretPos.x && pixCoord.x < caretPos.x + caretSize.x)
    {
        if (pixCoord.y > caretPos.y && pixCoord.y < caretPos.y + caretSize.y)
        {
            finalCol = vec4(1,1,1,1);
        }
    }

    FragColor = finalCol;
}