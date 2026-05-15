shader_type canvas_item;

uniform sampler2D noise_img; // Une texture de bruit pour la distorsion
uniform float speed : hint_range(0.0, 2.0) = 0.5;
uniform float intensity : hint_range(0.0, 1.0) = 0.1;

void fragment() {
    // Calcul du mouvement UV basé sur le temps
    vec2 scrolling_uv = UV + vec2(TIME * speed, TIME * speed);
    
    // Récupération de la distorsion via une texture de bruit
    vec4 noise = texture(noise_img, scrolling_uv);
    
    // Décalage de la texture principale par le bruit
    vec2 distorted_uv = UV + (noise.rg - 0.5) * intensity;
    
    // Affichage de la texture finale
    COLOR = texture(TEXTURE, distorted_uv);
}