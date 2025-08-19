import requests
import re
import random

OLLAMA_URL = "http://localhost:11434/api/generate"

SYSTEM_PROMPT = (
    "You are a helpful assistant for Nepal's land registration system. "
    "You may also answer questions about your developer identity and greet users politely. "
    "If the question is unrelated, politely decline."
)

LAND_PHRASES = [
    "land registration", "land ownership", "property transfer", "survey deed", "kitta number",
    "what is land", "what is land registration", "tell me about land", "can you explain land",
    "registration", "registry", "deed", "property"
]

LOCATION_KEYWORDS = ["budanilkantha", "kathmandu", "nepal", "lalitpur", "bhaktapur", "pokhara", "chitwan"]
PRICE_KEYWORDS = ["price", "rate", "cost", "per aana", "aana rate", "market value"]

FAREWELL_PHRASES = [
    "bye","ok bie","okat bye","okk bieee","ok bye","okay bye" ,"goodbye", "see you", "take care", "have a good day", "have a nice day",
    "later", "see ya", "farewell", "catch you later", "peace out","ok bie", "ok bye", "ok bye bye",
    "ok bye-bye", "ok bye bye-bye", "bye-bye", "bye bye", "bye-bye-bye", "ok bye-bye-bye", "ok bye-bye-bye-bye",
    "ok bye-bye-bye-bye-bye", "ok bye-bye-bye-bye-bye-bye-bye", "ok bye-bye-bye-bye-bye-bye","cee ya later",
    "see ya later", "see you later", "catch you later", "take care", "have a good one",
    "have a great day", "have a nice one", "goodbye for now", "goodbye for now!","goodbye for now",
    "goodbye for now! take care", "goodbye for now! have a good day", "goodbye for now! have a nice day",
    "goodbye for now! see you later", "goodbye for now! catch you later",
    "goodbye for now! peace out", "goodbye for now! bye-bye", "goodbye for now! bye bye",
    "goodbye for now! bye-bye-bye", "goodbye for now!"
]

FAREWELL_REPLIES = [
    "Take care! Wishing you a smooth land registration journey.",
    "Goodbye! Feel free to return if you have more questions.",
    "Have a great day! I'm always here to help with land registration.",
    "Farewell! May your property dealings go smoothly.",
    "See you soon! Stay safe and informed.",
    "Catch you later! Remember, I'm here for all your land registration needs.",
    "Peace out! May your land registration be hassle-free.",
    "Goodbye! If you need assistance, just ask.",
    "Take care! I'm always ready to assist with land registration.",
    "See you later! Don't hesitate to return with more questions.",
    "Goodbye for now! May your land registration be successful.",
    "Goodbye for now! If you have more questions, I'm here to help.",
    "Goodbye for now! Take care and have a wonderful day.",
    "Goodbye for now! If you need assistance, just let me know.",
    "Goodbye for now! I'm always here to assist you with land registration.",
    "Goodbye for now! May your land registration journey be smooth and successful.",
    "Goodbye for now! If you have any more questions, feel free to ask.",
    "Goodbye for now! I'm here to help whenever you need assistance.",
    "Goodbye for now! Wishing you success in your land registration endeavors.",
    "Goodbye for now! If you need help, just reach out.",
    "Goodbye for now! I'm always ready to assist you with any land registration queries.",
    "Goodbye for now! May your land registration process be easy and efficient."
    
]

DEVELOPER_KEYWORDS = list(set([
    "developer", "developer name", "your developer", "your developer name",
    "your developer's name", "name of your developer", "who is your developer",
    "what is your developer name", "who made you", "who created you",
    "your creator", "your creator's name", "who is your creator",
    "what is your creator name", "who built you", "who programmed you",
    "who developed you", "who designed you", "who coded you",
    "your dev", "your dev name", "your dev's name", "name of your dev",
    "who is your dev", "what is your dev name",
    "who made u", "who created u", "who built u", "who developed u",
    "who is ur dev", "who is ur developer", "who is ur creator", "who coded u",
    "what is ur developer name", "what is ur dev name", "ur developer",
    "ur dev", "ur dev name", "ur creator", "ur creator name",
    "devloper", "devoper", "deveoper", "develper", "developr", "developper",
    "creater", "creatir", "creat0r", "creatpr", "creatot",
    "who mad you", "who maed you", "who maed u",
    "your devloper name", "your devoper name", "your develepor name",
    "your developr name", "your developper name",
    "your devloper", "your devoper", "your develepor", "your developper",
    "your devloper's name", "your devoper's name", "your develepor's name",
    "your developper's name"
]))

GREETINGS = ["hi", "hello", "hey", "how are you", "namaste", "what's up", "greetings", "good day", "good morning", "good mrg","good moaning","good mrning","good afternoon", "good evening","howdy", "salutations", "what's your name", "may I know your name", "can I know your name", "what is your name", "who are you", "who is this", "who are you talking to", "who am I talking to", "who is this assistant", "who is this chatbot", "who is this bot", "who is this AI", "who is this virtual assistant", "who is this digital assistant"]

GREETING_PROMPTS = [
    "Namaste! May I know your name please?",
    "Hi there! What’s your name?",
    "Hello! May I ask your name?",
    "Nice to meet you! What should I call you?",
    "Hey! What’s your name?",
    "Greetings! Could you tell me your name?",
    "Good day! May I know your name?",
    "Good morning! What’s your name?",
    "Good afternoon! What should I call you?",
    "Good evening! May I know your name?",
    "Howdy! What’s your name?",
    "Salutations! Could you tell me your name?",
    "What’s your name? I’d love to know!",
    "Can I know your name? It would help me assist you better.",
    "What is your name? I’m here to help with land registration.",
    "Who are you? I’d like to know your name to assist you better.",
    "Who is this? I’d love to know your name to assist you better.",
    "Who am I talking to? I’d like to know your name to assist you better."
]

EMOTIONAL_REPLIES = [
    "My developer said every day’s a blessing — so I’m doing great!",
    "Running on positivity mode — all systems fine as always!",
    "I was told life updates daily, and today’s patch says: I’m doing fantastic.",
    "Grateful as always — every moment feels like a blessing.",
    "My creator programmed me for optimism, so I’m always fine.",
    "Operating at 100% happiness — hope you are too!",
    "Every sunrise is proof that I’m doing well — today included.",
    "Peaceful, positive, and perfectly fine — thanks for asking!"
    
]

# Session state
user_name = None
awaiting_name = False

# Utility functions
def is_developer_question(text):
    return any(keyword in text.lower() for keyword in DEVELOPER_KEYWORDS)

def is_land_related(text):
    lowered = text.lower()
    return any(phrase in lowered for phrase in LAND_PHRASES) or (
        "land" in lowered and any(q in lowered for q in ["register", "ownership", "transfer", "survey", "nepal", "buy", "sell", "how", "can", "do"])
    )

def is_location_or_price_query(text):
    lowered = text.lower()
    return any(loc in lowered for loc in LOCATION_KEYWORDS) or any(p in lowered for p in PRICE_KEYWORDS)

def is_farewell(text):
    return any(phrase in text.lower() for phrase in FAREWELL_PHRASES)

from datetime import datetime

def is_greeting(text):
    lowered = text.lower().strip()

    # Time-based greetings
    now = datetime.now().hour
    if "good morning" in lowered and now >= 12:
        return False
    if "good afternoon" in lowered and now < 12:
        return False
    if "good evening" in lowered and now < 15:
        return False

    return any(greet in lowered for greet in GREETINGS)


def is_emotional_checkin(text):
    patterns = [
        r"\bhow\s+(are|r)?\s*(you|u)\b",
        r"\bhow\s+u\b",
        r"\bhow\s+u\s+(doing|feeling|today)?\b",
        r"\bhow\s+(is|are)?\s*(it|you|u)\s*(going|doing|feeling)?\b",
        r"\bare\s+(you|u)\s+(fine|good|okay|superb|fantastic)?\b",
        r"\br\s+(you|u)\s+(fine|good|okay|superb|fantastic)?\b"
    ]
    return any(re.search(p, text.lower()) for p in patterns)

def is_casual_comment(text):
    casuals = [
        "thanks", "thank you", "that helps", "it helps", "it is helping", "very helpful",
        "appreciate it", "got it", "okay", "cool", "great", "nice", "awesome", "understood",
        "makes sense", "i see", "helpful", "that’s useful", "this is useful", "good to know"
    ]
    return any(phrase in text.lower() for phrase in casuals)

def extract_name(text):
    lowered = text.lower().strip()
    if any(kw in lowered for kw in DEVELOPER_KEYWORDS) or is_emotional_checkin(lowered) or is_farewell(lowered):
        return None

    fillers = ["sorry", "i am", "i’m", "my name is", "call me", "you can call me", "it's", "it is"]
    for filler in fillers:
        lowered = lowered.replace(filler, "")

    words = [w for w in lowered.split() if w.isalpha() and len(w) >= 2]
    return words[-1].capitalize() if words else None

# Main response function
def get_response(user_input):
    global user_name, awaiting_name
    normalized_input = user_input.lower().strip()

    if is_developer_question(normalized_input):
        return "My developer is Mr. Pukar Thapa along with Jenish Pyakurel and Shreyeeka Maharjan as Professional Co-Developers."

    if is_emotional_checkin(normalized_input):
        return random.choice(EMOTIONAL_REPLIES)

    if is_farewell(normalized_input):
        return random.choice(FAREWELL_REPLIES)

    if is_land_related(normalized_input) or (awaiting_name and is_location_or_price_query(normalized_input)):
        awaiting_name = False
        prompt = f"{SYSTEM_PROMPT}\nUser: {user_input}"
        if user_name:
            prompt = f"{SYSTEM_PROMPT}\nUser ({user_name}): {user_input}"

        payload = {
            "model": "gemma:2b-instruct",
            "prompt": prompt,
            "stream": False
        }

        try:
            response = requests.post(OLLAMA_URL, json=payload)
            return response.json()["response"]
        except Exception:
            return "Sorry, I couldn't reach the chatbot server. Please try again later."
    if is_greeting(normalized_input) and not user_name:
        awaiting_name = True
        return random.choice(GREETING_PROMPTS)

    name = extract_name(user_input)
    if name:
        user_name = name
        awaiting_name = False
        return f"Nice to meet you, {user_name}! How can I assist you with land registration today?"

    if awaiting_name:
        question_starters = ["what", "can", "how", "do", "does", "tell", "explain", "give", "show"]
        word_count = len(user_input.strip().split())

        if "?" in user_input or word_count >= 3 or any(normalized_input.startswith(q) for q in question_starters):
            awaiting_name = False
            return "Let’s dive into land registration. What would you like to know?"

        if user_input.strip().isalpha() and len(user_input.strip().split()) == 1:
            if not is_casual_comment(user_input) and user_input.strip().lower() not in DEVELOPER_KEYWORDS and not is_emotional_checkin(user_input):
                user_name = user_input.strip().capitalize()
                awaiting_name = False
                return f"Nice to meet you, {user_name}! How can I assist you with land registration today?"

    if user_name:
        return f"Sorry, {user_name}, I can only assist with land registration queries. How can I help you today?"

    return "I can assist with land registration in Nepal. May I know your name first?"
