# app.py

from flask import Flask, request, jsonify, render_template
from flask_cors import CORS
from chat import get_response

app = Flask(__name__)
CORS(app)  # This will allow requests from any origin

@app.route("/")
def home():
    return render_template("index.html")

@app.route("/chat", methods=["POST"])
def chat():
    user_input = request.json.get("message", "")
    reply = get_response(user_input)
    return jsonify({"reply": reply})

if __name__ == "__main__":
    app.run(debug=True)
