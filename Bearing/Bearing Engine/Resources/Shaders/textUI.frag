#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColour;

uniform vec2 caretPos;
uniform vec2 caretSize;

uniform sampler2D texture0;

void main()
{
    vec4 finalCol = mainColour * texture(texture0, texCoord);

    if (gl_FragCoord.x > caretPos.x && gl_FragCoord.x < caretPos.x + caretSize.x)
    {
        if (gl_FragCoord.y > caretPos.y && gl_FragCoord.y < caretPos.y + caretSize.y)
        {
            finalCol = vec4(1,0,0,1);
        }
    }

    FragColor = finalCol;
}