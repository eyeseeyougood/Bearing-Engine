#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColour;
uniform sampler2D textAtlas;

void main()
{
    FragColor = mainColour * texture(textAtlas, texCoord);

    //if (FragColor.a < 0.01)
    //    FragColor = vec4(0.0,0.2,0.6,1.0);
}