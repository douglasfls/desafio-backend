![Github Actions Status](https://github.com/github/docs/actions/workflows/main.yml/badge.svg?branch=main)

Desafio Backend
=

### Como executar a aplicação?

Para iniciar a aplicação basta iniciar o Aspire Host

``` commandline
dotnet run --project DesafioBackend.AppHost
```

### Quais os pré-requisitos para executar a aplicação?

- dotnet 9
- docker

### Estrutura da Aplicação:

Dentro da solution existe uma segmentação de pastas para ajudar a navegação e a leitura dos componentes.

### aspire
Dentro desta pasta encontram-se componentes focados em auxiliar a execução da aplicação principal. Sendo assim este projeto contem o arquivo de AppHost com a configuração e as features de discovery.
Outro projeto peculiar são os migrations services que é um serviço responsável por aplicar as migrações no banco de dados. 

### tests
Contém um projeto de teste de integração que eu inciei para testar o xunit com aspire, mas não cheguei a fazer uma cobertura completa ou muito elaborada nos endpoints.

### src / building-blocks
Contém os elmentos core da aplicação que num contexto maior possuiria itens como classes de dominios, mensagens, patterns, validators, contantes, interface com o banco de dados e afins.

### src / application
Contém as implementações de negócio voltadas ao funcionamento esperado e os endpoints da aplicação web.

### src / entrypoints
Onde se localizam os pontos de entrada do software como api rest, grpc, console e afins.

## Web UI
Como o template do dotnet 9 não possui uma web UI padrão, então optei por usar o Scalar que pode ser acessado através do path `/scalar/v1` 

## Validações
As validações são feitas través de um EndpointFilter que efetua a validação utilizando FluentValidation

## Onde está o docker compose?
Bom... não fiz, mas para gerar ele é possível gerar ele a partir do aspirate.
Para instalar o aspirate
``` commandline
dotnet tool install --global aspirate
```
E no diretório do `DesafioBackend.AppHost`
``` commandline
aspirate generate --output-format compose 
```
ou simplesmente use o script `gernerate-docker-compose.sh`

Aspirate irá pedir para definir um password e após isso selecione o Postgres, MigrationService e ApiService. Após isso defina um username e um password para o postgres e ao final do processo o resultado estará na pasta `aspirate-output`

