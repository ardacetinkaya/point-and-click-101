shader_type canvas_item;
render_mode unshaded;

uniform float noise_scale : hint_range(1.0, 10.0) = 4.0;
uniform float irregularity : hint_range(0.0, 1.0) = 0.55;
uniform float smoke_opacity : hint_range(0.0, 1.0) = 0.65;

float random_value(vec2 position) {
    return fract(
        sin(dot(position, vec2(12.9898, 78.233))) * 43758.5453
    );
}

float noise_value(vec2 position) {
    vec2 cell = floor(position);
    vec2 local_position = fract(position);

    float bottom_left = random_value(cell);
    float bottom_right = random_value(cell + vec2(1.0, 0.0));
    float top_left = random_value(cell + vec2(0.0, 1.0));
    float top_right = random_value(cell + vec2(1.0, 1.0));

    vec2 smooth_position =
        local_position * local_position * (3.0 - 2.0 * local_position);

    return mix(
        mix(bottom_left, bottom_right, smooth_position.x),
        mix(top_left, top_right, smooth_position.x),
        smooth_position.y
    );
}

void fragment() {
    vec2 centered_uv = UV * 2.0 - 1.0;

    float primary_noise = noise_value(
        UV * noise_scale +
        vec2(TIME * 0.05, -TIME * 0.03)
    );

    float detail_noise = noise_value(
        UV * noise_scale * 2.1 +
        vec2(-TIME * 0.025, TIME * 0.04)
    );

    float distorted_distance =
        length(centered_uv) +
        (primary_noise - 0.5) * irregularity +
        (detail_noise - 0.5) * irregularity * 0.35;

    float irregular_mask =
        1.0 - smoothstep(0.45, 1.0, distorted_distance);

    vec4 texture_color = texture(TEXTURE, UV);

    float final_alpha =
        COLOR.a *
        texture_color.a *
        irregular_mask *
        smoke_opacity;

    vec3 smoke_color =
        COLOR.rgb *
        mix(vec3(0.65), vec3(1.0), primary_noise);

    COLOR = vec4(smoke_color, final_alpha);
}