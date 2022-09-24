import flask

app = flask.Flask(__name__)

@app.post('/')
def post():
    print(flask.request.form['logs'])
    return '', 201


if __name__ == '__main__':
    app.run()
