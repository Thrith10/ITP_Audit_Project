
<!-- PROJECT LOGO -->
<br />



<div align="center">
  </a>

<h3 align="center">ITP 2024</h3>

  <p align="center">
    The project involves the development of an in-house automated audit engagement and stuff training monitoring systems which comprises a total of two primary systems in the form of a web and mobile application. The systems will be developed through the joint collaboration of the academic supervisors of SIT, the industry supervisor of PKF-CAP LLP, as well as Team 9 (SE) of the ITP module.
    <br />
    <a href="https://github.com/russelpwq/ITP_Audit_Project_2024"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/russelpwq/ITP_Audit_Project_2024">View Demo</a>
    ·
    <a href="https://github.com/russelpwq/ITP_Audit_Project_2024/issues">Report Bug</a>
    ·
    <a href="https://github.com/russelpwq/ITP_Audit_Project_2024/issues">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

[![Product Name Screen Shot][product-screenshot]](https://example.com)

The project is twofold. The first part requires the development of a web-based application to automate the creation of new/continuing engagement
jobs, tracking of manpower hours, monitoring engagement performance and archival of engagements. The system will send automatic email reminders
for actions from employees, serve as a repository of all required forms and has dashboard function to allow PKF to monitor and track the status of
each engagement job. The second part is a mobile application for users to register attendance of courses, take post-course quizzes and complete
feedback form. The app should have dashboard function for visibility and data is downloadable to spreadsheet for analysis. 

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ACKNOWLEDGMENTS -->
## The Development Team

* Russel Poon Wei Quan
* Tan Jin Hao
* Khoo Jun Wei
* Reness Ravichandra
* Hoe Jessaryn

<p align="right">(<a href="#readme-top">back to top</a>)</p>


### Built With

* ![Python][Python-url]
* ![Flask][Flask-url]
* ![HTML5][HTML5-url]
* ![CSS3][CSS3-url]
* ![Javascript][Javascript-url]
* [![Bootstrap][Bootstrap.com]][Bootstrap-url]
* [![JQuery][JQuery.com]][JQuery-url]
* ![MongoDB][MongoDB-url]
* ![MariaDB][MariaDB-url]


<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

This section is intended to provide you with all the information you need to get started with and use the software
effectively. To get a local copy up and running follow these simple example steps. 

### Prerequisites

This guide will be referencing PyCharm as the IDE of choice, and assumes you have MariaDB configured.

* npm
  ```sh
  npm install npm@latest -g
  ```

### Installation

1. Clone the repo
   ```sh
   git clone https://github.com/russelpwq/DBMS2023.git
   ```
2. Install packages through the "requirements.txt" file, there will be a prompt within the PyCharm IDE as shown below
  ![image](https://github.com/russelpwq/DBMS2023/assets/113176680/62f297b0-eea4-462c-930d-b2f2bd3fad09)

3. Enter the SQLALCHEMY_DATABASE_URI in `config.py` based on your MariaDB configuration
![image](https://github.com/russelpwq/DBMS2023/assets/113176680/e5933329-a1e2-4e06-9821-c78a0e48de34)

4. Select the 'run.py' file and run the application

### Troubleshooting

Occasionally, there may be errors when the database models have been edited.

1. Delete the "migrations" folder in the project directory

2. Open command prompt, and navigate into the directory containing the project as shown below

   
![image](https://github.com/russelpwq/DBMS2023/assets/113176680/ec11defa-9ba5-4980-aa02-b9f9c4591801)

3. Run the following command
   ```sh
   venv\Scripts\activate
   ```
      
5. Run the following command, and a new migrations folder should be created
   ```sh
   flask db init
   ```

6. Run the following command, to generate the initial migration
   ```sh
   flask db migrate
   ```
7. Run the following command to apply the changes
   ```sh
   flask db upgrade
   ```

8. Run the application (run.py) and it should work
   
<!-- USAGE EXAMPLES -->
## Usage

Use this space to show useful examples of how a project can be used. Additional screenshots, code examples and demos work well in this space. You may also link to more resources.

_For more examples, please refer to the [Documentation](https://example.com)_

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- ROADMAP -->
## Roadmap

See the [open issues](https://github.com/russelpwq/DBMS2023/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[DB logo]: https://github.com/russelpwq/DBMS2023/assets/113176680/963a9f48-2c91-4c91-93a7-ce65492aa318

[issues-shield]: https://img.shields.io/github/issues/github_username/repo_name.svg?style=for-the-badge
[issues-url]: https://github.com/russelpwq/DBMS2023/issues
[product-screenshot]: images/screenshot.png
[MariaDB-url]: https://img.shields.io/badge/MariaDB-003545?style=for-the-badge&logo=mariadb&logoColor=white
[Python-url]: https://img.shields.io/badge/Python-FFD43B?style=for-the-badge&logo=python&logoColor=blue
[Javascript-url]: https://img.shields.io/badge/JavaScript-323330?style=for-the-badge&logo=javascript&logoColor=F7DF1E
[HTML5-url]: https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white
[CSS3-url]: https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white
[Flask-url]: https://img.shields.io/badge/Flask-000000?style=for-the-badge&logo=flask&logoColor=white
[MongoDB-url]: https://img.shields.io/badge/MongoDB-4EA94B?style=for-the-badge&logo=mongodb&logoColor=white
[Laravel.com]: https://img.shields.io/badge/Laravel-FF2D20?style=for-the-badge&logo=laravel&logoColor=white
[Laravel-url]: https://laravel.com
[Bootstrap.com]: https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white
[Bootstrap-url]: https://getbootstrap.com
[JQuery.com]: https://img.shields.io/badge/jQuery-0769AD?style=for-the-badge&logo=jquery&logoColor=white
[JQuery-url]: https://jquery.com 

<p align="right">(<a href="#readme-top">back to top</a>)</p>
