# bot.py
import os

import discord
from discord.ext.commands import Bot
from dotenv import load_dotenv

load_dotenv()
TOKEN = os.getenv('DISCORD_TOKEN')
GUILD = os.getenv('DISCORD_GUILD')

bot = Bot(command_prefix = '!')

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

@bot.command()
async def play(ctx):
    intro_Message = discord.Embed(title="**Hello! Welcome to the test monster killing game!**",
                    description="Please select an action!")
    sentMessage = await ctx.send(embed=intro_Message)


    emojis = {
        'attack': "⚔",
        'defend': "🛡",
        'heal': "💖"
    }
    for emoji in emojis:
        await sentMessage.add_reaction(emojis['attack'])

    reaction, user = await bot.wait_for('reaction_add')
    print(reaction)
        #wait_for

bot.run(TOKEN)