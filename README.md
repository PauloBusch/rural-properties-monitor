# Projeto Hackathon para Monitoramento de Propriedades Rurais

## Sumário

- [Visão Geral](#visão-geral)
- [Componentes](#componentes)
- [Fluxo Geral de Dados](#fluxo-geral-de-dados)
- [Infraestrutura como Código (IaC)](#infraestrutura-como-código-iac)
   - [Como iniciar todos os serviços](#como-iniciar-todos-os-serviços)
   - [Como iniciar apenas serviços específicos](#como-iniciar-apenas-serviços-específicos)
   - [Como parar e remover os containers](#como-parar-e-remover-os-containers)
- [Kubernetes & Minikube](#kubernetes--minikube)
- [CI/CD Workflow](#cicd-workflow-minikube--self-hosted-runner)
   - [Como instalar um novo agente](#como-instalar-um-novo-agente-self-hosted-runner)
- [Autores](#autores)

## Visão Geral

O sistema é composto por uma arquitetura baseada em microsserviços, orientada a eventos e preparada para ingestão e análise de dados de sensores em propriedades rurais. O diagrama abaixo representa os principais componentes e seus fluxos de comunicação.

A proposta é permitir que dados coletados em campo (sensores) sejam ingeridos, armazenados, processados e posteriormente consumidos por produtores rurais através de uma API centralizada.

![Diagrama da Arquitetura](architecture-diagram.drawio.png)

## Componentes

### 👤 Rural Producer

Representa o usuário final (produtor rural), que consome os dados e insights por meio da **Analytics API**.

---

### 🔷 Analytics API

Serviço responsável por consolidar e disponibilizar dados analíticos ao usuário final.

Funções principais:

* Receber requisições HTTP do produtor rural
* Consultar dados em batch em outros serviços
* Utilizar o **Redis** como cache para melhorar performance
* Orquestrar chamadas para:

  * **Properties API** (dados cadastrais)
  * **Ingress API** (dados de sensores)

---

### 🟥 Redis

Utilizado como camada de cache para a **Analytics API**, reduzindo latência e evitando consultas repetidas aos serviços dependentes.

---

### 🔷 Properties API

Responsável pelo gerenciamento de dados relacionados às propriedades rurais.

Funções principais:

* Cadastro e consulta de propriedades
* Persistência dos dados no **MongoDB**

---

### 🟪 MongoDB

Banco de dados NoSQL utilizado para armazenar os dados estruturais e cadastrais das propriedades.

---

### 🔷 Ingress API

Serviço responsável pela ingestão e processamento dos dados vindos dos sensores.

Funções principais:

* Consumir eventos provenientes do **Kafka**
* Processar dados de telemetria
* Persistir séries temporais no **InfluxDB**
* Disponibilizar dados para consumo da **Analytics API**

---

### 🟦 InfluxDB

Banco de dados de séries temporais utilizado para armazenar os dados dos sensores processados pela **Ingress API**.

Funções principais:

* Armazenamento otimizado para dados de séries temporais
* Consultas eficientes baseadas em tempo
* Retenção automática de dados
* Interface web para visualização e consulta (http://localhost:8086)

Dados armazenados:
* Umidade do solo
* Temperatura
* Precipitação
* Timestamp dos sensores

---

### 🟧 Kafka

Plataforma de mensageria utilizada como broker de eventos entre os sensores e a **Ingress API**.

Benefícios:

* Comunicação assíncrona
* Maior resiliência
* Escalabilidade no processamento de eventos

---

### 📟 Sensors

Representam dispositivos de campo responsáveis pela coleta de dados (ex: temperatura, umaidade, localização, etc.).

Esses dados são enviados para o Kafka, simulando um cenário real de IoT.

---

### 🛡️ Keycloak

Responsável pela autenticação e autorização dos usuários e serviços.

Funções principais:

* Gerenciamento de identidade (IAM)
* Emissão e validação de tokens (OAuth2 / OpenID Connect)
* Integração com banco PostgreSQL

---

### 🐘 PostgreSQL

Banco de dados utilizado pelo **Keycloak** para persistência de usuários, credenciais e configurações de segurança.

---

## Fluxo Geral de Dados

1. Sensores enviam dados → **Kafka**
2. **Ingress API** consome os eventos e armazena no **InfluxDB**
3. **Properties API** gerencia dados cadastrais no **MongoDB**
4. **Analytics API** consulta os dois serviços em batch
5. Resultados são cacheados no **Redis**
6. O produtor rural consome os dados via requisições HTTP

## Infraestrutura como Código (IaC)

O projeto utiliza Docker Compose para orquestrar todos os serviços. O arquivo principal [`docker-compose.yml`](docker-compose.yml) está na raiz do projeto e inclui os arquivos de definição de cada serviço localizados na pasta [`iac`](iac/):

- [`iac/analytics-docker-compose.yml`](iac/analytics-docker-compose.yml)
- [`iac/ingress-docker-compose.yml`](iac/ingress-docker-compose.yml)
- [`iac/keycloak-docker-compose.yml`](iac/keycloak-docker-compose.yml)
- [`iac/properties-docker-compose.yml`](iac/properties-docker-compose.yml)
- [`iac/sensors-docker-compose.yml`](iac/sensors-docker-compose.yml)

### Como iniciar todos os serviços

No terminal, acesse a raiz do projeto (onde está o arquivo `docker-compose.yml`) e execute:

```sh
docker compose up -d
```

Isso irá iniciar todos os serviços definidos nos arquivos de compose incluídos.

### Como iniciar apenas serviços específicos

Você pode subir apenas um serviço (e suas dependências) usando:

```sh
docker compose up -d <nome-do-serviço>
```

Por exemplo, para subir apenas o serviço de sensores:

```sh
docker compose up -d sensors
```

> Certifique-se de que os arquivos de compose individuais estejam devidamente configurados com os serviços necessários.

### Como parar e remover os containers

Para parar todos os containers:

```sh
docker compose stop
```

Para parar e remover todos os containers, redes e volumes criados:

```sh
docker compose down
```

Você também pode usar essas opções com arquivos de compose personalizados usando a opção `-f`.

## Kubernetes & Minikube

Esta seção mostra como rodar os microsserviços em um cluster Kubernetes local usando Minikube.

### O que é Minikube?
Minikube executa clusters Kubernetes localmente, ideal para desenvolvimento e testes.

### Organização dos Manifests

Os manifests estão em [`k8s/`](k8s/):

- [`k8s/influxdb/`](k8s/influxdb/) — InfluxDB (Deployment, Service, PVC)
- [`k8s/kafka/`](k8s/kafka/) — Kafka (Deployment, Service)
- [`k8s/zookeeper/`](k8s/zookeeper/) — Zookeeper (Deployment, Service)
- [`k8s/ingress/`](k8s/ingress/) — Ingress API (Deployment, Service)

### Como rodar no Minikube

1. Inicie o Minikube:
   ```sh
   minikube start
   ```
2. Construa as imagens Docker dentro do Minikube:

   #### Para Bash:
   ```sh
   eval $(minikube docker-env)
   docker compose build
   ```

   #### Para PowerShell:
   ```sh
   minikube docker-env | Invoke-Expression
   docker compose build
   ```
   Ou manualmente:
   ```sh
   docker build -t ingress:latest ./src/Ingress
   ```
3. Aplique os manifests:
   ```sh
   kubectl apply -f k8s/ --recursive
   ```
   Ou aplique arquivos individuais conforme necessário.
4. Verifique pods e serviços:
   ```sh
   kubectl get pods
   kubectl get svc
   ```
5. Acesse a Ingress API:
   ```sh
   minikube service ingress-api
   ```
6. Abra o dashboard do Minikube (interface web para monitoramento):
   ```sh
   minikube dashboard
   ```
7. Parar o Minikube:
   ```sh
   minikube stop
   ```
8. Remover o cluster Minikube:
   ```sh
   minikube delete
   ```

### Dicas rápidas para troubleshooting no Minikube/Kubernetes

- Remover recursos: `kubectl delete -f <arquivo.yaml>`
- Ver logs de pods: `kubectl logs <nome-do-pod>`
- Verificar status dos pods: `kubectl get pods`
- Port-forward para acessar serviços localmente: `kubectl port-forward svc/<serviço> <porta-local>:<porta-serviço>`
- Escalar pods alterando `replicas` nos Deployments e aplicando novamente: `kubectl apply -f <deployment.yaml>`
- Verificar eventos e erros: `kubectl describe pod <nome-do-pod>`
- Verificar serviços expostos: `kubectl get svc`

## CI/CD Workflow (Minikube + Self-hosted Runner)

O projeto utiliza CI/CD automatizado para build, deploy e atualização do cluster Minikube local via GitHub Actions com runner self-hosted.


### Como funciona o workflow

```mermaid
graph TD
   A[Dev faz push] --> B[GitHub]
   B --> C[GitHub Actions]
   C --> D[Runner self-hosted]
   D --> E[dotnet test com cobertura]
   E --> F[Gerar relatório de cobertura]
   F --> G[Build imagens (docker compose)]
   G --> H[Aplicar manifests (kubectl apply)]
   H --> I[Minikube atualizado]
```

Passos do pipeline:

1. O desenvolvedor faz push para o GitHub.
2. O GitHub aciona o workflow.
3. O runner self-hosted executa o pipeline na sua máquina.
4. Executa os testes automatizados com cobertura de código.
5. Gera o relatório de cobertura (lcov).
6. Constrói todas as imagens dos serviços usando Docker Compose.
7. Aplica todos os manifests do Kubernetes (kubectl apply -f k8s/ --recursive).
8. O Minikube executa a nova versão automaticamente.


### Como instalar um novo agente (self-hosted runner)

1. No GitHub, acesse o repositório do projeto.
2. Vá em **Settings** → **Actions** → **Runners** → **New self-hosted runner**.
3. Siga as instruções para baixar, configurar e rodar o agente (Linux recomendado). Veja o [guia oficial](https://docs.github.com/pt/actions/hosting-your-own-runners/adding-self-hosted-runners).
4. O agente precisa ter Docker, kubectl e Minikube instalados e acessíveis.
5. O runner ficará disponível no GitHub para executar os workflows.

Se precisar de mais agentes, repita o processo em outras máquinas.

Erros comuns:
- Runner não acessa cluster: verifique se o contexto do kubectl é o minikube.
- Docker não encontrado: verifique se o runner está usando o ambiente do Minikube.
- Pods antigos: use `kubectl rollout restart deployment`.

Se precisar de mais agentes, repita o processo em outras máquinas.

Erros comuns:
- Runner não acessa cluster: verifique se o contexto do kubectl é o minikube
- Docker não encontrado: verifique se o runner está usando o ambiente do Minikube
- Pods antigos: use kubectl rollout restart deployment

## Autores

- [Paulo](https://github.com/paulobusch)
- [Geovanne](https://github.com/gehcosta)
- [Letícia](https://github.com/leticia-kojima)
- [Matheus](https://github.com/M4theusVieir4)
- [Marcelo](https://github.com/marceloalvees)
