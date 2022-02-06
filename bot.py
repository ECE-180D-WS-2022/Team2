# bot.py
import os

import discord
from discord.ext.commands import Bot
from dotenv import load_dotenv

import game

load_dotenv()
TOKEN = os.getenv('DISCORD_TOKEN')
GUILD = os.getenv('DISCORD_GUILD')

# intents = discord.Intents.default()
# intents.members = True

bot = Bot(command_prefix = '!', )#intents=intents)

# On CLI side
@bot.event
async def on_ready():
    for guild in bot.guilds:
        if guild.name == GUILD:
            break

    print(
        f'{bot.user} is connected to the following guild:\n'
        f'{guild.name}(id: {guild.id})'
    )
    print(discord.__version__)

# !play
@bot.command()
async def play(ctx):
    # Say intro message
    intro_Message = discord.Embed(title="**Hello! Welcome to the test monster killing game!**",
                    description="Please select an action!")
    sentMessage = await ctx.send(embed=intro_Message)

    emojis = {
        "⚔": "attack",
        "🛡": "defend",
        "💖": "heal"
    }

    # Display possible actions as emojis
    for k in emojis.keys():
        await sentMessage.add_reaction(k)

    # Wait for selection
    reaction, user = await bot.wait_for('reaction_add')
    print(reaction)
    
    # Extract emoji selection to keyword
    action = emojis[reaction]

    # Complete action
    #doAction(action)



bot.run(TOKEN)