#version 330 core
in vec2 aPosition;
in vec2 aTexCoord;

uniform vec2 position;
uniform float rot;
uniform vec2 scale;

out vec2 pos;
out vec2 texCoord;

void main()
{
    vec2 newPos = aPosition;

    vec3 final = vec3(newPos, 0.0);

    gl_Position = vec4(aPosition, 0.0, 0.5);
    
    texCoord = aTexCoord;
}