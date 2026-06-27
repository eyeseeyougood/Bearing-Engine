#version 330 core
out vec4 FragColor;

void main()
{
    FragColor = vec4(gl_FragCoord.z/2.0, gl_FragCoord.z/2.0, gl_FragCoord.z/2.0, 1.0);
}