class Player:
    def __init__(self):
        self.max_bombs = 2
        self.max_shields = 3
        self.hp_bullet = 5     # the hp reduction for bullet
        self.hp_AI = 10        # the hp reduction for AI action
        self.hp_bomb = 5
        self.hp_rain = 5
        self.max_shield_health = 30
        self.max_bullets = 6
        self.max_hp = 100

        self.num_deaths = 0

        self.hp = self.max_hp
        self.num_bullets = self.max_bullets
        self.num_bombs = self.max_bombs
        self.hp_shield = 0
        self.num_shield = self.max_shields

        # list of quadrants where rain has been started by the bomb of this player
        self.rain_list = []

    def __str__(self):
        return str(self.get_dict())

    def get_dict(self):
        data = {
            'bullets': self.num_bullets,
            'bombs': self.num_bombs,
            'hp': self.hp,
            'deaths': self.num_deaths,
            'shields': self.num_shield,
            'shield_hp': self.hp_shield
        }
        return data

    def update_state(self, state_data):
        """Update player state using incoming state data."""
        self.hp = state_data.get('hp', self.hp)  # Default to current value if not provided
        self.num_bullets = state_data.get('bullets', self.num_bullets)
        self.num_bombs = state_data.get('bombs', self.num_bombs)
        self.hp_shield = state_data.get('shield_hp', self.hp_shield)
        self.num_deaths = state_data.get('deaths', self.num_deaths)
        self.num_shield = state_data.get('shields', self.num_shield)

    def player_get_shot(self):
        self.hp -= self.hp_bullet

    def player_get_AI(self):
        self.hp -= self.hp_AI
