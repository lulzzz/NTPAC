import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch, Redirect} from "react-router-dom";
import { Grid, PageHeader } from 'react-bootstrap';

import './App.css';

import CaptureList from "./components/CaptureList";
import CaptureDetail from "./components/CaptureDetail";

import { CaptureApi } from "ntpac_api";


export default class App extends Component {
    constructor(props) {
      super(props);
      this.state = {
          captures: []
      };
      this.captureApi = new CaptureApi();
    }

    render = () => (
        <Router>
            <Grid>
                <div className="App">
                <PageHeader>
                    <a href="/" style={{"textDecoration": "none"}}>NTPAC <small>WebUI</small></a>
                </PageHeader>

                <Switch>
                    <Route exact path="/captures" render={() => <CaptureList captures={this.state.captures}/>} />
                    <Route path="/captures/:captureId" component={CaptureDetail} />
                    <Redirect to="/captures" />
                </Switch>
            </div>
        </Grid>
            
        </Router>
    ); 

   componentDidMount() {       
        this.captureApi.captureGetAll((error, captures, response) => {
            if (error) {
                console.error(error);
                return;
            }
            const newState = Object.assign({}, this.state, {
                captures: captures
            });
            this.setState(newState);
        });
    }
}
