MAX_PLAYERS = 2
MAX_MONSTERS = 5

PLAYER_HP = 10

class Game:
    def __init__(self):
        self.num_players = 0
        self.num_monsters = 0
        self.turn = 0
    
    def take_turn(self):
        if self.num_players < 1:
            print("Error: No players have joined the game")
    
    def doAction(self, action="Oops!"):
        print(action)



# class Unit:
#     def __init__(self):
#         #self.hp = 
#         # Every unit will have hp and mana

# class Player(Unit):
#     def __init__(self):
#         # type of player [Person, Remote]
#         # 

#     # action
#         # attack
#         # defend
#         # heal

# # Remote players will still be players
# # The way the game receives data will be different

# class Monster(Unit):
#     def __init__(self):