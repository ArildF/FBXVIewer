#version 330 core
in vec2 UV;
in vec3 fragmentColor;
in vec3 EyeDirection_cameraSpace;
in vec3 LightDirection_cameraSpace;
in vec3 Normal_cameraSpace;
in vec3 Position_worldSpace;
out vec3 color;

uniform sampler2D diffuseTextureSampler;
uniform mat4 M;
uniform mat4 V;
uniform vec3 LightPosition_worldSpace;

void main()
{
  vec3 LightColor = vec3(1,1,1);
  float LightPower = 5000f;
  
  float y = 1.0 - UV.y;
  vec3 diffuse = texture(diffuseTextureSampler, vec2(UV.x, y)).rgb;
  vec3 ambient = vec3(0.1, 0.1, 0.1) * diffuse;
  vec3 specular = vec3(0.3, 0.3, 0.3);
  
  float lightDistance = length(LightPosition_worldSpace - Position_worldSpace);
  
  vec3 normal = normalize(Normal_cameraSpace);
  vec3 lightDirection = normalize(LightDirection_cameraSpace);
  
  float cosTheta = clamp(dot(normal, lightDirection), 0, 1);
  
  vec3 eyeVector = normalize(EyeDirection_cameraSpace);
  vec3 reflection = reflect(-lightDirection, normal);
  
  float cosAlpha = clamp(dot(eyeVector, reflection), 0, 1);
  
  //color = ambient;
  color = ambient + 
  diffuse * LightColor * LightPower * cosTheta / (lightDistance * lightDistance) +
  specular * LightColor * LightPower * pow(cosAlpha, 5) / (lightDistance * lightDistance);
}