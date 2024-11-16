from enum import Enum

class Player:
    def __init__(self, id) -> None:
        self.id: int = id
        self.max_bombs: int = 2
        self.max_shields: int = 3
        self.hp_bullet: int = 5     # the hp reduction for bullet
        self.hp_AI: int = 10        # the hp reduction for AI action
        self.hp_bomb: int = 5
        self.hp_rain: int = 5   # this one havent implement - deen added
        self.max_shield_health: int = 30
        self.max_bullets: int = 6
        self.max_hp: int = 100

        self.death: int = 0
        self.hp: int = self.max_hp
        self.num_bullets: int = self.max_bullets
        self.num_bombs: int = self.max_bombs
        self.hp_shield: int = 0
        self.num_shield: int = self.max_shields
        self.enemyDetected: int = 0
        self.action = "none"
        self.kill = 0

    def update_state(self, action, visibility):
        self.action = action
        self.enemyDetected = visibility
        print("actionID: ", self.action)
        print("isEnemyVisible: ", self.enemyDetected)

    def is_enemy_detected(self, detected):
        return detected == 1

    def reduce_hp(self, dmg):
        if self.hp_shield > 0:
            # will be neg if dmg > remaining shield hp, hence deal remaining dmg to health
            remaining_shield_hp = self.hp_shield - dmg
            if remaining_shield_hp > 0:
                self.hp_shield = remaining_shield_hp
            else: 
                self.hp_shield = 0
                self.hp = self.hp - abs(remaining_shield_hp)
        else:
            self.hp = self.hp - dmg

        if self.hp <= 0:
            print("respawn")
            self.respawn()

    def shield_activate(self):
        if (self.hp_shield > 0 or self.num_shield <= 0):
            pass
        else:
            self.hp_shield = self.max_shield_health
            self.num_shield -= 1

    def shoot(self):
        if (self.num_bullets > 0):
            self.num_bullets -= 1

    def bomb_throw(self):
        if (self.num_bombs > 0):
            self.num_bombs -= 1

    def reload(self):
        if (self.num_bullets <= 0):
            self.num_bullets = self.max_bullets

    def respawn(self):
        if self.hp <= 0:
            self.hp = self.max_hp
            self.hp_shield = 0
            self.num_shield = self.num_shield
            self.num_bullets = self.num_bullets
            self.num_bombs = self.num_bombs
            self.death += 1
        

class GestureID(Enum):
    Idle = 0
    Shoot = 1
    Bomb = 2
    Basketball = 3
    Bowling = 4
    Volley = 5
    Soccer= 6
    Shield = 7
    Reload = 8
    Camera = 9
    Logout = 10