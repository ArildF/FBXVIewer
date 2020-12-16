#version 330 core
in vec2 UV;
in vec3 fragmentColor;
in vec3 EyeDirection_tangentSpace;
in vec3 LightDirection_tangentSpace;
in vec3 Position_worldSpace;
in vec3 vertexNormal;
out vec3 color;

uniform sampler2D normalTextureSampler;
uniform sampler2D diffuseTextureSampler;
uniform sampler2D specularTextureSampler;
uniform mat4 M;
uniform mat4 V;
uniform vec3 LightPosition_worldSpace;
uniform float LightPower;
uniform float Ambient;
uniform float SpecularStrength;

void main()
{
    
  vec3 LightColor = vec3(1,1,1);
  
  float y = 1.0 - UV.y;
  vec3 diffuse = texture(diffuseTextureSampler, vec2(UV.x, y)).rgb;
  vec3 ambient = Ambient * diffuse;
  vec3 specular = texture(specularTextureSampler, vec2(UV.x, y)).rgb * SpecularStrength;
  
  vec3 packedNormal = texture(normalTextureSampler, vec2(UV.x, y)).xyz;
  packedNormal.xy = packedNormal.xy * 2.0 - 1.0;
  
  vec3 normal_TangentSpace = normalize(packedNormal);
  
  float lightDistance = length(LightPosition_worldSpace - Position_worldSpace);
  
  vec3 normal = normal_TangentSpace;
  vec3 lightDirection = normalize(LightDirection_tangentSpace);
  
  float cosTheta = clamp(dot(normal, lightDirection), 0, 1);
  
  vec3 eyeVector = normalize(EyeDirection_tangentSpace);
  vec3 reflection = reflect(-lightDirection, normal);
  
  float cosAlpha = clamp(dot(eyeVector, reflection), 0, 1);
  
  //color = fragmentColor;
  color = ambient + 
    diffuse * LightColor * LightPower * cosTheta / (lightDistance * lightDistance) +
    specular * LightColor * LightPower * pow(cosAlpha, 5) / (lightDistance * lightDistance);
}