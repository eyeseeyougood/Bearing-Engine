#version 330 core
in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aNormal;

uniform vec2 offset;

void main()
{
    gl_Position = vec4(aPosition + offset, 1.0, 1.0);
}