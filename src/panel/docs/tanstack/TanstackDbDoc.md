---
title: Overview
id: overview
---

# TanStack DB - Documentation

Welcome to the TanStack DB documentation.

TanStack DB is the reactive client store for your API. It solves the problems of building fast, modern apps, helping you:

- avoid endpoint sprawl and network waterfalls by loading data into normalized collections
- optimise client performance with sub-millisecond live queries and real-time reactivity
- take the network off the interaction path with instant optimistic writes

Data loading is optimized. Interactions feel instantaneous. Your backend stays simple and your app stays blazing fast. No matter how much data you load.

## Remove the complexity from building fast, modern apps

TanStack DB lets you query your data however your components need it, with a blazing-fast local query engine, real-time reactivity and instant optimistic updates.

Instead of choosing between the least of two evils:

1. **view-specific APIs** - complicating your backend and leading to network waterfalls
2. **load everything and filter** - leading to slow loads and sluggish client performance

TanStack DB enables a new way:

3. **normalized collections** - keep your backend simple
4. **query-driven sync** - optimizes your data loading
5. **sub-millisecond live queries** - keep your app fast and responsive

It extends TanStack Query with collections, live queries and optimistic mutations, working seamlessly with REST APIs, sync engines, or any data source.

## Contents

- [How it works](#how-it-works) &mdash; understand the TanStack DB development model and how the pieces fit together
- [API reference](#api-reference) &mdash; for the primitives and function interfaces
- [Usage examples](#usage-examples) &mdash; examples of common usage patterns
- [More info](#more-info) &mdash; where to find support and more information

## How it works

TanStack DB works by:

- [defining collections](#defining-collections) typed sets of objects that can be populated with data
- [using live queries](#using-live-queries) to query data from/across collections
- [making optimistic mutations](#making-optimistic-mutations) using transactional mutators

```tsx
// Define collections to load data into
const todoCollection = createCollection({
  // ...your config
  onUpdate: updateMutationFn,
})

const Todos = () => {
  // Bind data using live queries
  const { data: todos } = useLiveQuery((q) =>
    q.from({ todo: todoCollection }).where(({ todo }) => todo.completed)
  )

  const complete = (todo) => {
    // Instantly applies optimistic state
    todoCollection.update(todo.id, (draft) => {
      draft.completed = true
    })
  }

  return (
    <ul>
      {todos.map((todo) => (
        <li key={todo.id} onClick={() => complete(todo)}>
          {todo.text}
        </li>
      ))}
    </ul>
  )
}
```

### Defining collections

Collections are typed sets of objects that can be populated with data. They're designed to de-couple loading data into your app from binding data to your components.

Collections can be populated in many ways, including:

- fetching data, for example [from API endpoints using TanStack Query](https://tanstack.com/query/latest)
- syncing data, for example [using a sync engine like ElectricSQL](https://electric-sql.com/)
- storing local data, for example [using localStorage for user preferences and settings](./collections/local-storage-collection.md) or [in-memory client data or UI state](./collections/local-only-collection.md)
- from live collection queries, creating [derived collections as materialised views](#using-live-queries)

Once you have your data in collections, you can query across them using live queries in your components.

#### Sync Modes

Collections support three sync modes to optimize data loading:

- **Eager mode** (default): Loads entire collection upfront. Best for <10k rows of mostly static data like user preferences or small reference tables.
- **On-demand mode**: Loads only what queries request. Best for large datasets (>50k rows), search interfaces, and catalogs where most data won't be accessed.
- **Progressive mode**: Loads query subset immediately, syncs full dataset in background. Best for collaborative apps needing instant first paint AND sub-millisecond queries.

With on-demand mode, your component's query becomes the API call:

```tsx
const productsCollection = createCollection(
  queryCollectionOptions({
    queryKey: ['products'],
    queryFn: async (ctx) => {
      // Query predicates passed automatically in ctx.meta
      const params = parseLoadSubsetOptions(ctx.meta?.loadSubsetOptions)
      return api.getProducts(params) // e.g., GET /api/products?category=electronics&price_lt=100
    },
    syncMode: 'on-demand', // ← Enable query-driven sync
  })
)
```

TanStack DB automatically collapses duplicate requests, performs delta loading when expanding queries, optimizes joins into minimal batched requests, and respects your TanStack Query cache policies. You often end up with _fewer_ network requests than custom view-specific APIs.

See the [Query Collection documentation](./collections/query-collection.md#queryfn-and-predicate-push-down) for full predicate mapping details.

### Using live queries

Live queries are used to query data out of collections. Live queries are reactive: when the underlying data changes in a way that would affect the query result, the result is incrementally updated and returned from the query, triggering a re-render.

TanStack DB live queries are implemented using [d2ts](https://github.com/electric-sql/d2ts), a TypeScript implementation of differential dataflow. This allows the query results to update _incrementally_ (rather than by re-running the whole query). This makes them blazing fast, usually sub-millisecond, even for highly complex queries.

**Performance:** Updating one row in a sorted 100,000-item collection completes in ~0.7ms on an M1 Pro MacBook—fast enough that optimistic updates feel truly instantaneous, even with complex queries and large datasets.

Live queries support joins across collections. This allows you to:

1. load normalised data into collections and then de-normalise it through queries; simplifying your backend by avoiding the need for bespoke API endpoints that match your client
2. join data from multiple sources; for example, syncing some data out of a database, fetching some other data from an external API and then joining these into a unified data model for your front-end code

Every query returns another collection which can _also_ be queried.

For more details on live queries, see the [Live Queries](./guides/live-queries.md) documentation.

### Making optimistic mutations

Collections support `insert`, `update` and `delete` operations. When called, by default they trigger the corresponding `onInsert`, `onUpdate`, `onDelete` handlers which are responsible for writing the mutation to the backend.

```ts
// Define collection with persistence handlers
const todoCollection = createCollection({
  id: "todos",
  // ... other config
  onUpdate: async ({ transaction }) => {
    const { original, changes } = transaction.mutations[0]
    await api.todos.update(original.id, changes)
  },
})

// Immediately applies optimistic state
todoCollection.update(todo.id, (draft) => {
  draft.completed = true
})
```

The collection maintains optimistic state separately from synced data. When live queries read from the collection, they see a local view that overlays the optimistic mutations on top of the immutable synced data.

The optimistic state is held until the handler resolves, at which point the data is persisted to the server and synced back. If the handler throws an error, the optimistic state is rolled back.

For more complex mutations, you can create custom actions with `createOptimisticAction` or custom transactions with `createTransaction`. See the [Mutations guide](./guides/mutations.md) for details.

### Uni-directional data flow

This combines to support a model of uni-directional data flow, extending the redux/flux style state management pattern beyond the client, to take in the server as well:

<figure>
  <a href="https://raw.githubusercontent.com/TanStack/db/main/docs/unidirectional-data-flow.lg.png" target="_blank">
    <img src="https://raw.githubusercontent.com/TanStack/db/main/docs/unidirectional-data-flow.png" />
  </a>
</figure>

With an instant inner loop of optimistic state, superseded in time by the slower outer loop of persisting to the server and syncing the updated server state back into the collection.

## API reference

### Collections

TanStack DB provides several built-in collection types for different data sources and use cases. Each collection type has its own detailed documentation page:

#### Built-in Collection Types

**Fetch Collections**

- **[QueryCollection](./collections/query-collection.md)** &mdash; Load data into collections using TanStack Query for REST APIs and data fetching.

**Sync Collections**

- **[ElectricCollection](./collections/electric-collection.md)** &mdash; Sync data into collections from Postgres using ElectricSQL's real-time sync engine.

- **[TrailBaseCollection](./collections/trailbase-collection.md)** &mdash; Sync data into collections using TrailBase's self-hosted backend with real-time subscriptions.

- **[RxDBCollection](./collections/rxdb-collection.md)** &mdash; Integrate with RxDB for offline-first local persistence with powerful replication and sync capabilities.

- **[PowerSyncCollection](./collections/powersync-collection.md)** &mdash; Sync with PowerSync's SQLite-based database for offline-first persistence with real-time synchronization with PostgreSQL, MongoDB, and MySQL backends.

**Local Collections**

- **[LocalStorageCollection](./collections/local-storage-collection.md)** &mdash; Store small amounts of local-only state that persists across sessions and syncs across browser tabs.

- **[LocalOnlyCollection](./collections/local-only-collection.md)** &mdash; Manage in-memory client data or UI state that doesn't need persistence or cross-tab sync.

#### Collection Schemas

All collections optionally (though strongly recommended) support adding a `schema`.

If provided, this must be a [Standard Schema](https://standardschema.dev) compatible schema instance, such as [Zod](https://zod.dev), [Valibot](https://valibot.dev), [ArkType](https://arktype.io), or [Effect](https://effect.website/docs/schema/introduction/).

**What schemas do:**

1. **Runtime validation** - Ensures data meets your constraints before entering the collection
2. **Type transformations** - Convert input types to rich output types (e.g., string → Date)
3. **Default values** - Automatically populate missing fields
4. **Type safety** - Infer TypeScript types from your schema

**Example:**
```typescript
const todoSchema = z.object({
  id: z.string(),
  text: z.string(),
  completed: z.boolean().default(false),
  created_at: z.string().transform(val => new Date(val)),  // string → Date
  priority: z.number().default(0)
})

const collection = createCollection(
  queryCollectionOptions({
    schema: todoSchema,
    // ...
  })
)

// Users provide simple inputs
collection.insert({
  id: "1",
  text: "Buy groceries",
  created_at: "2024-01-01T00:00:00Z"  // string
  // completed and priority filled automatically
})

// Collection stores and returns rich types
const todo = collection.get("1")
console.log(todo.created_at.getFullYear())  // It's a Date!
console.log(todo.completed)  // false (default)
```

The collection will use the schema for its type inference. If you provide a schema, you cannot also pass an explicit type parameter (e.g., `createCollection<Todo>()`).

**Learn more:** See the [Schemas guide](./guides/schemas.md) for comprehensive documentation on schema validation, type transformations, and best practices.

#### Creating Custom Collection Types

You can create your own collection types by implementing the `Collection` interface found in [`../packages/db/src/collection/index.ts`](https://github.com/TanStack/db/blob/main/packages/db/src/collection/index.ts).

See the existing implementations in [`../packages/db`](https://github.com/TanStack/db/tree/main/packages/db), [`../packages/query-db-collection`](https://github.com/TanStack/db/tree/main/packages/query-db-collection), [`../packages/electric-db-collection`](https://github.com/TanStack/db/tree/main/packages/electric-db-collection) and [`../packages/trailbase-db-collection`](https://github.com/TanStack/db/tree/main/packages/trailbase-db-collection) for reference. Also see the [Collection Options Creator guide](./guides/collection-options-creator.md) for a pattern to create reusable collection configuration factories.

### Live queries

#### `useLiveQuery` hook

Use the `useLiveQuery` hook to assign live query results to a state variable in your React components:

```ts
import { useLiveQuery } from '@tanstack/react-db'
import { eq } from '@tanstack/db'

const Todos = () => {
  const { data: todos } = useLiveQuery((q) =>
    q
      .from({ todo: todoCollection })
      .where(({ todo }) => eq(todo.completed, false))
      .orderBy(({ todo }) => todo.created_at, 'asc')
      .select(({ todo }) => ({
        id: todo.id,
        text: todo.text
      }))
  )

  return <List items={ todos } />
}
```

You can also query across collections with joins:

```ts
import { useLiveQuery } from '@tanstack/react-db'
import { eq } from '@tanstack/db'

const Todos = () => {
  const { data: todos } = useLiveQuery((q) =>
    q
      .from({ todos: todoCollection })
      .join(
        { lists: listCollection },
        ({ todos, lists }) => eq(lists.id, todos.listId),
        'inner'
      )
      .where(({ lists }) => eq(lists.active, true))
      .select(({ todos, lists }) => ({
        id: todos.id,
        title: todos.title,
        listName: lists.name
      }))
  )

  return <List items={ todos } />
}
```

#### `useLiveSuspenseQuery` hook

For React Suspense support, use `useLiveSuspenseQuery`. This hook suspends rendering during initial data load and guarantees that `data` is always defined:

```tsx
import { useLiveSuspenseQuery } from '@tanstack/react-db'
import { Suspense } from 'react'

const Todos = () => {
  // data is always defined - no need for optional chaining
  const { data: todos } = useLiveSuspenseQuery((q) =>
    q
      .from({ todo: todoCollection })
      .where(({ todo }) => eq(todo.completed, false))
  )

  return <List items={ todos } />
}

const App = () => (
  <Suspense fallback={<div>Loading...</div>}>
    <Todos />
  </Suspense>
)
```

See the [React Suspense section in Live Queries](./guides/live-queries#using-with-react-suspense) for detailed usage patterns and when to use `useLiveSuspenseQuery` vs `useLiveQuery`.

#### `queryBuilder`

You can also build queries directly (outside of the component lifecycle) using the underlying `queryBuilder` API:

```ts
import { createLiveQueryCollection, eq } from "@tanstack/db"

const completedTodos = createLiveQueryCollection({
  startSync: true,
  query: (q) =>
    q
      .from({ todo: todoCollection })
      .where(({ todo }) => eq(todo.completed, true)),
})

const results = completedTodos.toArray
```

Note also that:

1. the query results [are themselves a collection](#derived-collections)
2. the `useLiveQuery` automatically starts and stops live query subscriptions when you mount and unmount your components; if you're creating queries manually, you need to manually manage the subscription lifecycle yourself

See the [Live Queries](./guides/live-queries.md) documentation for more details.

### Transactional mutators

For more complex mutations beyond simple CRUD operations, TanStack DB provides `createOptimisticAction` and `createTransaction` for creating custom mutations with full control over the mutation lifecycle.

See the [Mutations guide](./guides/mutations.md) for comprehensive documentation on:

- Creating custom actions with `createOptimisticAction`
- Manual transactions with `createTransaction`
- Mutation merging behavior
- Controlling optimistic vs non-optimistic updates
- Handling temporary IDs
- Transaction lifecycle states

## Usage examples

Here we illustrate two common ways of using TanStack DB:

1. [using TanStack Query](#1-tanstack-query) with an existing REST API
2. [using the ElectricSQL sync engine](#2-electricsql-sync) for real-time sync with your existing API

> [!TIP]
> You can combine these patterns. One of the benefits of TanStack DB is that you can integrate different ways of loading data and handling mutations into the same app. Your components don't need to know where the data came from or goes.

### 1. TanStack Query

You can use TanStack DB with your existing REST API via TanStack Query.

The steps are to:

1. create [QueryCollection](./collections/query-collection.md)s that load data using TanStack Query
2. implement mutation handlers that handle mutations by posting them to your API endpoints

```tsx
import { useLiveQuery, createCollection } from "@tanstack/react-db"
import { queryCollectionOptions } from "@tanstack/query-db-collection"

// Load data into collections using TanStack Query.
// It's common to define these in a `collections` module.
const todoCollection = createCollection(
  queryCollectionOptions({
    queryKey: ["todos"],
    queryFn: async () => fetch("/api/todos"),
    getKey: (item) => item.id,
    schema: todoSchema, // any standard schema
    onInsert: async ({ transaction }) => {
      const { changes: newTodo } = transaction.mutations[0]

      // Handle the local write by sending it to your API.
      await api.todos.create(newTodo)
    },
    // also add onUpdate, onDelete as needed.
  })
)
const listCollection = createCollection(
  queryCollectionOptions({
    queryKey: ["todo-lists"],
    queryFn: async () => fetch("/api/todo-lists"),
    getKey: (item) => item.id,
    schema: todoListSchema,
    onInsert: async ({ transaction }) => {
      const { changes: newTodo } = transaction.mutations[0]

      // Handle the local write by sending it to your API.
      await api.todoLists.create(newTodo)
    },
    // also add onUpdate, onDelete as needed.
  })
)

const Todos = () => {
  // Read the data using live queries. Here we show a live
  // query that joins across two collections.
  const { data: todos } = useLiveQuery((q) =>
    q
      .from({ todo: todoCollection })
      .join(
        { list: listCollection },
        ({ todo, list }) => eq(list.id, todo.list_id),
        "inner"
      )
      .where(({ list }) => eq(list.active, true))
      .select(({ todo, list }) => ({
        id: todo.id,
        text: todo.text,
        status: todo.status,
        listName: list.name,
      }))
  )

  // ...
}
```

This pattern allows you to extend an existing TanStack Query application, or any application built on a REST API, with blazing fast, cross-collection live queries and local optimistic mutations with automatically managed optimistic state.

### 2. ElectricSQL sync

One of the most powerful ways of using TanStack DB is with a sync engine, for a fully local-first experience with real-time sync. This allows you to incrementally adopt sync into an existing app, whilst still handling writes with your existing API.

#### Why Sync Engines?

While TanStack DB works great with REST APIs, sync engines provide powerful benefits:

- **Easy real-time updates**: No WebSocket plumbing—write to your database and changes stream automatically to all clients
- **Automatic side-effects**: When a mutation triggers cascading changes across tables, all affected data syncs automatically without manual cache invalidation
- **Efficient delta updates**: Only changed rows cross the wire, making it practical to load large datasets client-side

This pattern enables the "load everything once" approach that makes apps like Linear and Figma feel instant.

Here, we illustrate this pattern using [ElectricSQL](https://electric-sql.com) as the sync engine.

```tsx
import type { Collection } from "@tanstack/db"
import type {
  MutationFn,
  PendingMutation,
  createCollection,
} from "@tanstack/react-db"
import { electricCollectionOptions } from "@tanstack/electric-db-collection"

export const todoCollection = createCollection(
  electricCollectionOptions({
    id: "todos",
    schema: todoSchema,
    // Electric syncs data using "shapes". These are filtered views
    // on database tables that Electric keeps in sync for you.
    shapeOptions: {
      url: "https://api.electric-sql.cloud/v1/shape",
      params: {
        table: "todos",
      },
    },
    getKey: (item) => item.id,
    schema: todoSchema,
    onInsert: async ({ transaction }) => {
      const response = await api.todos.create(transaction.mutations[0].modified)

      return { txid: response.txid }
    },
    // You can also implement onUpdate, onDelete as needed.
  })
)

const AddTodo = () => {
  return (
    <Button
      onClick={() => todoCollection.insert({ text: "🔥 Make app faster" })}
    />
  )
}
```

## React Native

When using TanStack DB with React Native, you need to install and configure a UUID generation library since React Native doesn't include crypto.randomUUID() by default.

Install the `react-native-random-uuid` package:

```bash
npm install react-native-random-uuid
```

Then import it at the entry point of your React Native app (e.g., in your `App.js` or `index.js`):

```javascript
import "react-native-random-uuid"
```

This polyfill provides the `crypto.randomUUID()` function that TanStack DB uses internally for generating unique identifiers.

## More info

If you have questions / need help using TanStack DB, let us know on the Discord or start a GitHub discussion:

- [`#db` channel in the TanStack discord](https://discord.gg/yjUNbvbraC)
- [GitHub discussions](https://github.com/tanstack/db/discussions)


---


---
title: Installation
id: installation
---

Each supported framework comes with its own package. Each framework package re-exports everything from the core `@tanstack/db` package.

## React

```sh
npm install @tanstack/react-db
```

TanStack DB is compatible with React v16.8+

## Solid

```sh
npm install @tanstack/solid-db
```

## Svelte

```sh
npm install @tanstack/svelte-db
```

## Vue

```sh
npm install @tanstack/vue-db
```

TanStack DB is compatible with Vue v3.3.0+

## Angular

```sh
npm install @tanstack/angular-db
```

TanStack DB is compatible with Angular v16.0.0+

## Vanilla JS

```sh
npm install @tanstack/db
```

Install the the core `@tanstack/db` package to use DB without a framework.

## Collection Packages

TanStack DB also provides specialized collection packages for different data sources and storage needs:

### Query Collection

For loading data using TanStack Query:

```sh
npm install @tanstack/query-db-collection
```

Use `queryCollectionOptions` to fetch data into collections using TanStack Query. This is perfect for REST APIs and existing TanStack Query setups.

### Local Collections

Local storage and in-memory collections are included with the framework packages:

- **LocalStorageCollection** - For persistent local data that syncs across browser tabs
- **LocalOnlyCollection** - For temporary in-memory data and UI state

Both use `localStorageCollectionOptions` and `localOnlyCollectionOptions` respectively, available from your framework package (e.g., `@tanstack/react-db`).

### Sync Engines

#### Electric Collection

For real-time sync with [ElectricSQL](https://electric-sql.com):

```sh
npm install @tanstack/electric-db-collection
```

Use `electricCollectionOptions` to sync data from Postgres databases through ElectricSQL shapes. Ideal for real-time, local-first applications.

#### TrailBase Collection

For syncing with [TrailBase](https://trailbase.io) backends:

```sh
npm install @tanstack/trailbase-db-collection
```

Use `trailBaseCollectionOptions` to sync records from TrailBase's Record APIs with built-in subscription support.

### RxDB Collection

For offline-first apps and local persistence with [RxDB](https://rxdb.info):

```sh
npm install @tanstack/rxdb-db-collection
```

Use `rxdbCollectionOptions` to bridge an [RxDB collection](https://rxdb.info/rx-collection.html) into TanStack DB.
This gives you reactive TanStack DB collections backed by RxDB's powerful local-first database, replication, and conflict handling features.


---


---
title: Quick Start
id: quick-start
---

TanStack DB is the reactive client-first store for your API. Stop building custom endpoints for every view—query your data however your components need it. This example will show you how to:

- **Load data** into collections using TanStack Query
- **Query data** with blazing-fast live queries
- **Mutate data** with instant optimistic updates

```tsx
import { createCollection, eq, useLiveQuery } from '@tanstack/react-db'
import { queryCollectionOptions } from '@tanstack/query-db-collection'

// Define a collection that loads data using TanStack Query
const todoCollection = createCollection(
  queryCollectionOptions({
    queryKey: ['todos'],
    queryFn: async () => {
      const response = await fetch('/api/todos')
      return response.json()
    },
    getKey: (item) => item.id,
    onUpdate: async ({ transaction }) => {
      const { original, modified } = transaction.mutations[0]
      await fetch(`/api/todos/${original.id}`, {
        method: 'PUT',
        body: JSON.stringify(modified),
      })
    },
  })
)

function Todos() {
  // Live query that updates automatically when data changes
  const { data: todos } = useLiveQuery((q) =>
    q.from({ todo: todoCollection })
     .where(({ todo }) => eq(todo.completed, false))
     .orderBy(({ todo }) => todo.createdAt, 'desc')
  )

  const toggleTodo = (todo) => {
    // Instantly applies optimistic state, then syncs to server
    todoCollection.update(todo.id, (draft) => {
      draft.completed = !draft.completed
    })
  }

  return (
    <ul>
      {todos.map((todo) => (
        <li key={todo.id} onClick={() => toggleTodo(todo)}>
          {todo.text}
        </li>
      ))}
    </ul>
  )
}
```

You now have collections, live queries, and optimistic mutations! Let's break this down further.

## Installation

```bash
npm install @tanstack/react-db @tanstack/query-db-collection
```

## 1. Create a Collection

Collections store your data and handle persistence. The `queryCollectionOptions` loads data using TanStack Query and defines mutation handlers for server sync:

```tsx
const todoCollection = createCollection(
  queryCollectionOptions({
    queryKey: ['todos'],
    queryFn: async () => {
      const response = await fetch('/api/todos')
      return response.json()
    },
    getKey: (item) => item.id,
    // Handle all CRUD operations
    onInsert: async ({ transaction }) => {
      const { modified: newTodo } = transaction.mutations[0]
      await fetch('/api/todos', {
        method: 'POST',
        body: JSON.stringify(newTodo),
      })
    },
    onUpdate: async ({ transaction }) => {
      const { original, modified } = transaction.mutations[0]
      await fetch(`/api/todos/${original.id}`, {
        method: 'PUT', 
        body: JSON.stringify(modified),
      })
    },
    onDelete: async ({ transaction }) => {
      const { original } = transaction.mutations[0]
      await fetch(`/api/todos/${original.id}`, { method: 'DELETE' })
    },
  })
)
```

## 2. Query with Live Queries

Live queries reactively update when data changes. They support filtering, sorting, joins, and transformations:

```tsx
function TodoList() {
  // Basic filtering and sorting
  const { data: incompleteTodos } = useLiveQuery((q) =>
    q.from({ todo: todoCollection })
     .where(({ todo }) => eq(todo.completed, false))
     .orderBy(({ todo }) => todo.createdAt, 'desc')
  )

  // Transform the data
  const { data: todoSummary } = useLiveQuery((q) =>
    q.from({ todo: todoCollection })
     .select(({ todo }) => ({
       id: todo.id,
       summary: `${todo.text} (${todo.completed ? 'done' : 'pending'})`,
       priority: todo.priority || 'normal'
     }))
  )

  return <div>{/* Render todos */}</div>
}
```

## 3. Optimistic Mutations

Mutations apply instantly and sync to your server. If the server request fails, changes automatically roll back:

```tsx
function TodoActions({ todo }) {
  const addTodo = () => {
    todoCollection.insert({
      id: crypto.randomUUID(),
      text: 'New todo',
      completed: false,
      createdAt: new Date(),
    })
  }

  const toggleComplete = () => {
    todoCollection.update(todo.id, (draft) => {
      draft.completed = !draft.completed
    })
  }

  const updateText = (newText) => {
    todoCollection.update(todo.id, (draft) => {
      draft.text = newText
    })
  }

  const deleteTodo = () => {
    todoCollection.delete(todo.id)
  }

  return (
    <div>
      <button onClick={addTodo}>Add Todo</button>
      <button onClick={toggleComplete}>Toggle</button>
      <button onClick={() => updateText('Updated!')}>Edit</button>
      <button onClick={deleteTodo}>Delete</button>
    </div>
  )
}
```

## Next Steps

You now understand the basics of TanStack DB! The collection loads and persists data, live queries provide reactive views, and mutations give instant feedback with automatic server sync.

Explore the docs to learn more about:

- **[Installation](./installation.md)** - All framework and collection packages
- **[Overview](./overview.md)** - Complete feature overview and examples
- **[Live Queries](./guides/live-queries.md)** - Advanced querying, joins, and aggregations


---

# COLLECTIONS

---


---
title: Query Collection
---

# Query Collection

Query collections provide seamless integration between TanStack DB and TanStack Query, enabling automatic synchronization between your local database and remote data sources.

## Overview

The `@tanstack/query-db-collection` package allows you to create collections that:

- Automatically fetch remote data via TanStack Query
- Support optimistic updates with automatic rollback on errors
- Handle persistence through customizable mutation handlers
- Provide direct write capabilities for directly writing to the sync store

## Installation

```bash
npm install @tanstack/query-db-collection @tanstack/query-core @tanstack/db
```

## Basic Usage

```typescript
import { QueryClient } from "@tanstack/query-core"
import { createCollection } from "@tanstack/db"
import { queryCollectionOptions } from "@tanstack/query-db-collection"

const queryClient = new QueryClient()

const todosCollection = createCollection(
  queryCollectionOptions({
    queryKey: ["todos"],
    queryFn: async () => {
      const response = await fetch("/api/todos")
      return response.json()
    },
    queryClient,
    getKey: (item) => item.id,
  })
)
```

## Configuration Options

The `queryCollectionOptions` function accepts the following options:

### Required Options

- `queryKey`: The query key for TanStack Query
- `queryFn`: Function that fetches data from the server
- `queryClient`: TanStack Query client instance
- `getKey`: Function to extract the unique key from an item

### Query Options

- `select`: Function that lets extract array items when they're wrapped with metadata
- `enabled`: Whether the query should automatically run (default: `true`)
- `refetchInterval`: Refetch interval in milliseconds (default: 0 — set an interval to enable polling refetching)
- `retry`: Retry configuration for failed queries
- `retryDelay`: Delay between retries
- `staleTime`: How long data is considered fresh
- `meta`: Optional metadata that will be passed to the query function context

### Collection Options

- `id`: Unique identifier for the collection
- `schema`: Schema for validating items
- `sync`: Custom sync configuration
- `startSync`: Whether to start syncing immediately (default: `true`)

### Persistence Handlers

- `onInsert`: Handler called before insert operations
- `onUpdate`: Handler called before update operations
- `onDelete`: Handler called before delete operations

## Extending Meta with Custom Properties

The `meta` option allows you to pass additional metadata to your query function. By default, Query Collections automatically include `loadSubsetOptions` in the meta object, which contains filtering, sorting, and pagination options for on-demand queries.

### Type-Safe Meta Access

The `ctx.meta.loadSubsetOptions` property is automatically typed as `LoadSubsetOptions` without requiring any additional imports or type assertions:

```typescript
import { parseLoadSubsetOptions } from "@tanstack/query-db-collection"

const collection = createCollection(
  queryCollectionOptions({
    queryKey: ["products"],
    syncMode: "on-demand",
    queryFn: async (ctx) => {
      // ✅ Type-safe access - no @ts-ignore needed!
      const options = parseLoadSubsetOptions(ctx.meta?.loadSubsetOptions)

      // Use the parsed options to fetch only what you need
      return api.getProducts(options)
    },
    queryClient,
    getKey: (item) => item.id,
  })
)
```

### Adding Custom Meta Properties

You can extend the meta type to include your own custom properties using TypeScript's module augmentation:

```typescript
// In a global type definition file (e.g., types.d.ts or global.d.ts)
declare module "@tanstack/query-db-collection" {
  interface QueryCollectionMeta {
    // Add your custom properties here
    userId?: string
    includeDeleted?: boolean
    cacheTTL?: number
  }
}
```

Once you've extended the interface, your custom properties are fully typed throughout your application:

```typescript
const collection = createCollection(
  queryCollectionOptions({
    queryKey: ["todos"],
    queryFn: async (ctx) => {
      // ✅ Both loadSubsetOptions and custom properties are typed
      const { loadSubsetOptions, userId, includeDeleted } = ctx.meta

      return api.getTodos({
        ...parseLoadSubsetOptions(loadSubsetOptions),
        userId,
        includeDeleted,
      })
    },
    queryClient,
    getKey: (item) => item.id,
    // Pass custom meta alongside Query Collection defaults
    meta: {
      userId: "user-123",
      includeDeleted: false,
    },
  })
)
```

### Important Notes

- The module augmentation pattern follows TanStack Query's official approach for typing meta
- `QueryCollectionMeta` is an interface (not a type alias), enabling proper TypeScript declaration merging
- Your custom properties are merged with the base `loadSubsetOptions` property
- All meta properties must be compatible with `Record<string, unknown>`
- The augmentation should be done in a file that's included in your TypeScript compilation

### Example: API Request Context

A common use case is passing request context to your query function:

```typescript
// types.d.ts
declare module "@tanstack/query-db-collection" {
  interface QueryCollectionMeta {
    authToken?: string
    locale?: string
    version?: string
  }
}

// collections.ts
const productsCollection = createCollection(
  queryCollectionOptions({
    queryKey: ["products"],
    queryFn: async (ctx) => {
      const { loadSubsetOptions, authToken, locale, version } = ctx.meta

      return api.getProducts({
        ...parseLoadSubsetOptions(loadSubsetOptions),
        headers: {
          Authorization: `Bearer ${authToken}`,
          "Accept-Language": locale,
          "API-Version": version,
        },
      })
    },
    queryClient,
    getKey: (item) => item.id,
    meta: {
      authToken: session.token,
      locale: "en-US",
      version: "v1",
    },
  })
)
```

## Persistence Handlers

You can define handlers that are called when mutations occur. These handlers can persist changes to your backend and control whether the query should refetch after the operation:

```typescript
const todosCollection = createCollection(
  queryCollectionOptions({
    queryKey: ["todos"],
    queryFn: fetchTodos,
    queryClient,
    getKey: (item) => item.id,

    onInsert: async ({ transaction }) => {
      const newItems = transaction.mutations.map((m) => m.modified)
      await api.createTodos(newItems)
      // Returning nothing or { refetch: true } will trigger a refetch
      // Return { refetch: false } to skip automatic refetch
    },

    onUpdate: async ({ transaction }) => {
      const updates = transaction.mutations.map((m) => ({
        id: m.key,
        changes: m.changes,
      }))
      await api.updateTodos(updates)
    },

    onDelete: async ({ transaction }) => {
      const ids = transaction.mutations.map((m) => m.key)
      await api.deleteTodos(ids)
    },
  })
)
```

### Controlling Refetch Behavior

By default, after any persistence handler (`onInsert`, `onUpdate`, or `onDelete`) completes successfully, the query will automatically refetch to ensure the local state matches the server state.

You can control this behavior by returning an object with a `refetch` property:

```typescript
onInsert: async ({ transaction }) => {
  await api.createTodos(transaction.mutations.map((m) => m.modified))

  // Skip the automatic refetch
  return { refetch: false }
}
```

This is useful when:

- You're confident the server state matches what you sent
- You want to avoid unnecessary network requests
- You're handling state updates through other mechanisms (like WebSockets)

## Utility Methods

The collection provides these utility methods via `collection.utils`:

- `refetch(opts?)`: Manually trigger a refetch of the query
  - `opts.throwOnError`: Whether to throw an error if the refetch fails (default: `false`)
  - Bypasses `enabled: false` to support imperative/manual refetching patterns (similar to hook `refetch()` behavior)
  - Returns `QueryObserverResult` for inspecting the result

## Direct Writes

Direct writes are intended for scenarios where the normal query/mutation flow doesn't fit your needs. They allow you to write directly to the synced data store, bypassing the optimistic update system and query refetch mechanism.

### Understanding the Data Stores

Query Collections maintain two data stores:

1. **Synced Data Store** - The authoritative state synchronized with the server via `queryFn`
2. **Optimistic Mutations Store** - Temporary changes that are applied optimistically before server confirmation

Normal collection operations (insert, update, delete) create optimistic mutations that are:

- Applied immediately to the UI
- Sent to the server via persistence handlers
- Rolled back automatically if the server request fails
- Replaced with server data when the query refetches

Direct writes bypass this system entirely and write directly to the synced data store, making them ideal for handling real-time updates from alternative sources.

### When to Use Direct Writes

Direct writes should be used when:

- You need to sync real-time updates from WebSockets or server-sent events
- You're dealing with large datasets where refetching everything is too expensive
- You receive incremental updates or server-computed field updates
- You need to implement complex pagination or partial data loading scenarios

### Individual Write Operations

```typescript
// Insert a new item directly to the synced data store
todosCollection.utils.writeInsert({
  id: "1",
  text: "Buy milk",
  completed: false,
})

// Update an existing item in the synced data store
todosCollection.utils.writeUpdate({ id: "1", completed: true })

// Delete an item from the synced data store
todosCollection.utils.writeDelete("1")

// Upsert (insert or update) in the synced data store
todosCollection.utils.writeUpsert({
  id: "1",
  text: "Buy milk",
  completed: false,
})
```

These operations:

- Write directly to the synced data store
- Do NOT create optimistic mutations
- Do NOT trigger automatic query refetches
- Update the TanStack Query cache immediately
- Are immediately visible in the UI

### Batch Operations

The `writeBatch` method allows you to perform multiple operations atomically. Any write operations called within the callback will be collected and executed as a single transaction:

```typescript
todosCollection.utils.writeBatch(() => {
  todosCollection.utils.writeInsert({ id: "1", text: "Buy milk" })
  todosCollection.utils.writeInsert({ id: "2", text: "Walk dog" })
  todosCollection.utils.writeUpdate({ id: "3", completed: true })
  todosCollection.utils.writeDelete("4")
})
```

### Real-World Example: WebSocket Integration

```typescript
// Handle real-time updates from WebSocket without triggering full refetches
ws.on("todos:update", (changes) => {
  todosCollection.utils.writeBatch(() => {
    changes.forEach((change) => {
      switch (change.type) {
        case "insert":
          todosCollection.utils.writeInsert(change.data)
          break
        case "update":
          todosCollection.utils.writeUpdate(change.data)
          break
        case "delete":
          todosCollection.utils.writeDelete(change.id)
          break
      }
    })
  })
})
```

### Example: Incremental Updates

When the server returns computed fields (like server-generated IDs or timestamps), you can use the `onInsert` handler with `{ refetch: false }` to avoid unnecessary refetches while still syncing the server response:

```typescript
const todosCollection = createCollection(
  queryCollectionOptions({
    queryKey: ["todos"],
    queryFn: fetchTodos,
    queryClient,
    getKey: (item) => item.id,

    onInsert: async ({ transaction }) => {
      const newItems = transaction.mutations.map((m) => m.modified)

      // Send to server and get back items with server-computed fields
      const serverItems = await api.createTodos(newItems)

      // Sync server-computed fields (like server-generated IDs, timestamps, etc.)
      // to the collection's synced data store
      todosCollection.utils.writeBatch(() => {
        serverItems.forEach((serverItem) => {
          todosCollection.utils.writeInsert(serverItem)
        })
      })

      // Skip automatic refetch since we've already synced the server response
      // (optimistic state is automatically replaced when handler completes)
      return { refetch: false }
    },

    onUpdate: async ({ transaction }) => {
      const updates = transaction.mutations.map((m) => ({
        id: m.key,
        changes: m.changes,
      }))
      const serverItems = await api.updateTodos(updates)

      // Sync server-computed fields from the update response
      todosCollection.utils.writeBatch(() => {
        serverItems.forEach((serverItem) => {
          todosCollection.utils.writeUpdate(serverItem)
        })
      })

      return { refetch: false }
    },
  })
)

// Usage is just like a regular collection
todosCollection.insert({ text: "Buy milk", completed: false })
```

### Example: Large Dataset Pagination

```typescript
// Load additional pages without refetching existing data
const loadMoreTodos = async (page) => {
  const newTodos = await api.getTodos({ page, limit: 50 })

  // Add new items without affecting existing ones
  todosCollection.utils.writeBatch(() => {
    newTodos.forEach((todo) => {
      todosCollection.utils.writeInsert(todo)
    })
  })
}
```

## Important Behaviors

### Full State Sync

The query collection treats the `queryFn` result as the **complete state** of the collection. This means:

- Items present in the collection but not in the query result will be deleted
- Items in the query result but not in the collection will be inserted
- Items present in both will be updated if they differ

### Empty Array Behavior

When `queryFn` returns an empty array, **all items in the collection will be deleted**. This is because the collection interprets an empty array as "the server has no items".

```typescript
// This will delete all items in the collection
queryFn: async () => []
```

### Handling Partial/Incremental Fetches

Since the query collection expects `queryFn` to return the complete state, you can handle partial fetches by merging new data with existing data:

```typescript
const todosCollection = createCollection(
  queryCollectionOptions({
    queryKey: ["todos"],
    queryFn: async ({ queryKey }) => {
      // Get existing data from cache
      const existingData = queryClient.getQueryData(queryKey) || []

      // Fetch only new/updated items (e.g., changes since last sync)
      const lastSyncTime = localStorage.getItem("todos-last-sync")
      const newData = await fetch(`/api/todos?since=${lastSyncTime}`).then(
        (r) => r.json()
      )

      // Merge new data with existing data
      const existingMap = new Map(existingData.map((item) => [item.id, item]))

      // Apply updates and additions
      newData.forEach((item) => {
        existingMap.set(item.id, item)
      })

      // Handle deletions if your API provides them
      if (newData.deletions) {
        newData.deletions.forEach((id) => existingMap.delete(id))
      }

      // Update sync time
      localStorage.setItem("todos-last-sync", new Date().toISOString())

      // Return the complete merged state
      return Array.from(existingMap.values())
    },
    queryClient,
    getKey: (item) => item.id,
  })
)
```

This pattern allows you to:

- Fetch only incremental changes from your API
- Merge those changes with existing data
- Return the complete state that the collection expects
- Avoid the performance overhead of fetching all data every time

### Direct Writes and Query Sync

Direct writes update the collection immediately and also update the TanStack Query cache. However, they do not prevent the normal query sync behavior. If your `queryFn` returns data that conflicts with your direct writes, the query data will take precedence.

To handle this properly:

1. Use `{ refetch: false }` in your persistence handlers when using direct writes
2. Set appropriate `staleTime` to prevent unnecessary refetches
3. Design your `queryFn` to be aware of incremental updates (e.g., only fetch new data)

## Complete Direct Write API Reference

All direct write methods are available on `collection.utils`:

- `writeInsert(data)`: Insert one or more items directly
- `writeUpdate(data)`: Update one or more items directly
- `writeDelete(keys)`: Delete one or more items directly
- `writeUpsert(data)`: Insert or update one or more items directly
- `writeBatch(callback)`: Perform multiple operations atomically
- `refetch(opts?)`: Manually trigger a refetch of the query

## QueryFn and Predicate Push-Down

When using `syncMode: 'on-demand'`, the collection automatically pushes down query predicates (where clauses, orderBy, limit, and offset) to your `queryFn`. This allows you to fetch only the data needed for each specific query, rather than fetching the entire dataset.

### How LoadSubsetOptions Are Passed

LoadSubsetOptions are passed to your `queryFn` via the query context's `meta` property:

```typescript
queryFn: async (ctx) => {
  // Extract LoadSubsetOptions from the context
  const { limit, offset, where, orderBy } = ctx.meta.loadSubsetOptions

  // Use these to fetch only the data you need
  // - where: filter expression (AST)
  // - orderBy: sort expression (AST)
  // - limit: maximum number of rows
  // - offset: number of rows to skip (for pagination)
  // ...
}
```

The `where` and `orderBy` fields are expression trees (AST - Abstract Syntax Tree) that need to be parsed. TanStack DB provides helper functions to make this easy.

### Expression Helpers

```typescript
import {
  parseWhereExpression,
  parseOrderByExpression,
  extractSimpleComparisons,
  parseLoadSubsetOptions,
} from '@tanstack/db'
// Or from '@tanstack/query-db-collection' (re-exported for convenience)
```

These helpers allow you to parse expression trees without manually traversing complex AST structures.

### Quick Start: Simple REST API

```typescript
import { createCollection } from '@tanstack/react-db'
import { queryCollectionOptions } from '@tanstack/query-db-collection'
import { parseLoadSubsetOptions } from '@tanstack/db'
import { QueryClient } from '@tanstack/query-core'

const queryClient = new QueryClient()

const productsCollection = createCollection(
  queryCollectionOptions({
    id: 'products',
    queryKey: ['products'],
    queryClient,
    getKey: (item) => item.id,
    syncMode: 'on-demand', // Enable predicate push-down

    queryFn: async (ctx) => {
      const { limit, offset, where, orderBy } = ctx.meta.loadSubsetOptions

      // Parse the expressions into simple format
      const parsed = parseLoadSubsetOptions({ where, orderBy, limit })

      // Build query parameters from parsed filters
      const params = new URLSearchParams()

      // Add filters
      parsed.filters.forEach(({ field, operator, value }) => {
        const fieldName = field.join('.')
        if (operator === 'eq') {
          params.set(fieldName, String(value))
        } else if (operator === 'lt') {
          params.set(`${fieldName}_lt`, String(value))
        } else if (operator === 'gt') {
          params.set(`${fieldName}_gt`, String(value))
        }
      })

      // Add sorting
      if (parsed.sorts.length > 0) {
        const sortParam = parsed.sorts
          .map(s => `${s.field.join('.')}:${s.direction}`)
          .join(',')
        params.set('sort', sortParam)
      }

      // Add limit
      if (parsed.limit) {
        params.set('limit', String(parsed.limit))
      }

      // Add offset for pagination
      if (offset) {
        params.set('offset', String(offset))
      }

      const response = await fetch(`/api/products?${params}`)
      return response.json()
    },
  })
)

// Usage with live queries
import { createLiveQueryCollection } from '@tanstack/react-db'
import { eq, lt, and } from '@tanstack/db'

const affordableElectronics = createLiveQueryCollection({
  query: (q) =>
    q.from({ product: productsCollection })
     .where(({ product }) => and(
       eq(product.category, 'electronics'),
       lt(product.price, 100)
     ))
     .orderBy(({ product }) => product.price, 'asc')
     .limit(10)
     .select(({ product }) => product)
})

// This triggers a queryFn call with:
// GET /api/products?category=electronics&price_lt=100&sort=price:asc&limit=10
// When paginating, offset is included: &offset=20
```

### Custom Handlers for Complex APIs

For APIs with specific formats, use custom handlers:

```typescript
queryFn: async (ctx) => {
  const { where, orderBy, limit } = ctx.meta.loadSubsetOptions

  // Use custom handlers to match your API's format
  const filters = parseWhereExpression(where, {
    handlers: {
      eq: (field, value) => ({
        field: field.join('.'),
        op: 'equals',
        value
      }),
      lt: (field, value) => ({
        field: field.join('.'),
        op: 'lessThan',
        value
      }),
      and: (...conditions) => ({
        operator: 'AND',
        conditions
      }),
      or: (...conditions) => ({
        operator: 'OR',
        conditions
      }),
    }
  })

  const sorts = parseOrderByExpression(orderBy)

  return api.query({
    filters,
    sort: sorts.map(s => ({
      field: s.field.join('.'),
      order: s.direction.toUpperCase()
    })),
    limit
  })
}
```

### GraphQL Example

```typescript
queryFn: async (ctx) => {
  const { where, orderBy, limit } = ctx.meta.loadSubsetOptions

  // Convert to a GraphQL where clause format
  const whereClause = parseWhereExpression(where, {
    handlers: {
      eq: (field, value) => ({
        [field.join('_')]: { _eq: value }
      }),
      lt: (field, value) => ({
        [field.join('_')]: { _lt: value }
      }),
      and: (...conditions) => ({ _and: conditions }),
      or: (...conditions) => ({ _or: conditions }),
    }
  })

  // Convert to a GraphQL order_by format
  const sorts = parseOrderByExpression(orderBy)
  const orderByClause = sorts.map(s => ({
    [s.field.join('_')]: s.direction
  }))

  const { data } = await graphqlClient.query({
    query: gql`
      query GetProducts($where: product_bool_exp, $orderBy: [product_order_by!], $limit: Int) {
        product(where: $where, order_by: $orderBy, limit: $limit) {
          id
          name
          category
          price
        }
      }
    `,
    variables: {
      where: whereClause,
      orderBy: orderByClause,
      limit
    }
  })

  return data.product
}
```

### Expression Helper API Reference

#### `parseLoadSubsetOptions(options)`

Convenience function that parses all LoadSubsetOptions at once. Good for simple use cases.

```typescript
const { filters, sorts, limit, offset } = parseLoadSubsetOptions(ctx.meta?.loadSubsetOptions)
// filters: [{ field: ['category'], operator: 'eq', value: 'electronics' }]
// sorts: [{ field: ['price'], direction: 'asc', nulls: 'last' }]
// limit: 10
// offset: 20 (for pagination)
```

#### `parseWhereExpression(expr, options)`

Parses a WHERE expression using custom handlers for each operator. Use this for complete control over the output format.

```typescript
const filters = parseWhereExpression(where, {
  handlers: {
    eq: (field, value) => ({ [field.join('.')]: value }),
    lt: (field, value) => ({ [`${field.join('.')}_lt`]: value }),
    and: (...filters) => Object.assign({}, ...filters)
  },
  onUnknownOperator: (operator, args) => {
    console.warn(`Unsupported operator: ${operator}`)
    return null
  }
})
```

#### `parseOrderByExpression(orderBy)`

Parses an ORDER BY expression into a simple array.

```typescript
const sorts = parseOrderByExpression(orderBy)
// Returns: [{ field: ['price'], direction: 'asc', nulls: 'last' }]
```

#### `extractSimpleComparisons(expr)`

Extracts simple AND-ed comparisons from a WHERE expression. Note: Only works for simple AND conditions.

```typescript
const comparisons = extractSimpleComparisons(where)
// Returns: [
//   { field: ['category'], operator: 'eq', value: 'electronics' },
//   { field: ['price'], operator: 'lt', value: 100 }
// ]
```

### Supported Operators

- `eq` - Equality (=)
- `gt` - Greater than (>)
- `gte` - Greater than or equal (>=)
- `lt` - Less than (<)
- `lte` - Less than or equal (<=)
- `and` - Logical AND
- `or` - Logical OR
- `in` - IN clause

### Using Query Key Builders

Create different cache entries for different filter combinations:

```typescript
const productsCollection = createCollection(
  queryCollectionOptions({
    id: 'products',
    // Dynamic query key based on filters
    queryKey: (opts) => {
      const parsed = parseLoadSubsetOptions(opts)
      const cacheKey = ['products']

      parsed.filters.forEach(f => {
        cacheKey.push(`${f.field.join('.')}-${f.operator}-${f.value}`)
      })

      if (parsed.limit) {
        cacheKey.push(`limit-${parsed.limit}`)
      }

      return cacheKey
    },
    queryClient,
    getKey: (item) => item.id,
    syncMode: 'on-demand',
    queryFn: async (ctx) => { /* ... */ },
  })
)
```

### Tips

1. **Start with `parseLoadSubsetOptions`** for simple use cases
2. **Use custom handlers** via `parseWhereExpression` for APIs with specific formats
3. **Handle unsupported operators** with the `onUnknownOperator` callback
4. **Log parsed results** during development to verify correctness


---


---
title: Electric Collection
---

# Electric Collection

Electric collections provide seamless integration between TanStack DB and ElectricSQL, enabling real-time data synchronization with your Postgres database through Electric's sync engine.

## Overview

The `@tanstack/electric-db-collection` package allows you to create collections that:
- Automatically sync data from Postgres via Electric shapes
- Support optimistic updates with transaction matching and automatic rollback on errors
- Handle persistence through customizable mutation handlers

## Installation

```bash
npm install @tanstack/electric-db-collection @tanstack/react-db
```

## Basic Usage

```typescript
import { createCollection } from '@tanstack/react-db'
import { electricCollectionOptions } from '@tanstack/electric-db-collection'

const todosCollection = createCollection(
  electricCollectionOptions({
    shapeOptions: {
      url: '/api/todos',
    },
    getKey: (item) => item.id,
  })
)
```

## Configuration Options

The `electricCollectionOptions` function accepts the following options:

### Required Options

- `shapeOptions`: Configuration for the ElectricSQL ShapeStream
  - `url`: The URL of your proxy to Electric

- `getKey`: Function to extract the unique key from an item

### Optional

- `id`: Unique identifier for the collection
- `schema`: Schema for validating items. Any Standard Schema compatible schema
- `sync`: Custom sync configuration

### Persistence Handlers

Handlers are called before mutations to persist changes to your backend:

- `onInsert`: Handler called before insert operations
- `onUpdate`: Handler called before update operations
- `onDelete`: Handler called before delete operations

Each handler should return `{ txid }` to wait for synchronization. For cases where your API can not return txids, use the `awaitMatch` utility function.

## Persistence Handlers & Synchronization

Handlers persist mutations to the backend and wait for Electric to sync the changes back. This prevents UI glitches where optimistic updates would be removed and then re-added. TanStack DB blocks sync data until the mutation is confirmed, ensuring smooth user experience.

### 1. Using Txid (Recommended)

The recommended approach uses PostgreSQL transaction IDs (txids) for precise matching. The backend returns a txid, and the client waits for that specific txid to appear in the Electric stream.

```typescript
const todosCollection = createCollection(
  electricCollectionOptions({
    id: 'todos',
    schema: todoSchema,
    getKey: (item) => item.id,
    shapeOptions: {
      url: '/api/todos',
      params: { table: 'todos' },
    },

    onInsert: async ({ transaction }) => {
      const newItem = transaction.mutations[0].modified
      const response = await api.todos.create(newItem)

      // Return txid to wait for sync
      return { txid: response.txid }
    },

    onUpdate: async ({ transaction }) => {
      const { original, changes } = transaction.mutations[0]
      const response = await api.todos.update({
        where: { id: original.id },
        data: changes
      })

      return { txid: response.txid }
    }
  })
)
```

### 2. Using Custom Match Functions

For cases where txids aren't available, use the `awaitMatch` utility function to wait for synchronization with custom matching logic:

```typescript
import { isChangeMessage } from '@tanstack/electric-db-collection'

const todosCollection = createCollection(
  electricCollectionOptions({
    id: 'todos',
    getKey: (item) => item.id,
    shapeOptions: {
      url: '/api/todos',
      params: { table: 'todos' },
    },

    onInsert: async ({ transaction, collection }) => {
      const newItem = transaction.mutations[0].modified
      await api.todos.create(newItem)

      // Use awaitMatch utility for custom matching
      await collection.utils.awaitMatch(
        (message) => {
          return isChangeMessage(message) &&
                 message.headers.operation === 'insert' &&
                 message.value.text === newItem.text
        },
        5000 // timeout in ms (optional, defaults to 3000)
      )
    }
  })
)
```

### 3. Using Simple Timeout

For quick prototyping or when you're confident about timing, you can use a simple timeout. This is crude but works as almost always the data will be synced back in under 2 seconds:

```typescript
const todosCollection = createCollection(
  electricCollectionOptions({
    id: 'todos',
    getKey: (item) => item.id,
    shapeOptions: {
      url: '/api/todos',
      params: { table: 'todos' },
    },

    onInsert: async ({ transaction }) => {
      const newItem = transaction.mutations[0].modified
      await api.todos.create(newItem)

      // Simple timeout approach
      await new Promise(resolve => setTimeout(resolve, 2000))
    }
  })
)
```

On the backend, you can extract the `txid` for a transaction by querying Postgres directly.

```ts
async function generateTxId(tx) {
  // The ::xid cast strips off the epoch, giving you the raw 32-bit value
  // that matches what PostgreSQL sends in logical replication streams
  // (and then exposed through Electric which we'll match against
  // in the client).
  const result = await tx.execute(
    sql`SELECT pg_current_xact_id()::xid::text as txid`
  )
  const txid = result.rows[0]?.txid

  if (txid === undefined) {
    throw new Error(`Failed to get transaction ID`)
  }

  return parseInt(txid as string, 10)
}
```

### Electric Proxy Example

Electric is typically deployed behind a proxy server that handles shape configuration, authentication and authorization. This provides better security and allows you to control what data users can access without exposing Electric to the client.


Here is an example proxy implementation using TanStack Starter:

```js
import { createServerFileRoute } from "@tanstack/react-start/server"
import { ELECTRIC_PROTOCOL_QUERY_PARAMS } from "@electric-sql/client"

// Electric URL
const baseUrl = 'http://.../v1/shape'

const serve = async ({ request }: { request: Request }) => {
  // ...check user authorization  
  const url = new URL(request.url)
  const originUrl = new URL(baseUrl)

  // passthrough parameters from electric client
  url.searchParams.forEach((value, key) => {
    if (ELECTRIC_PROTOCOL_QUERY_PARAMS.includes(key)) {
      originUrl.searchParams.set(key, value)
    }
  })

  // set shape parameters 
  // full spec: https://github.com/electric-sql/electric/blob/main/website/electric-api.yaml
  originUrl.searchParams.set("table", "todos")
  // Where clause to filter rows in the table (optional).
  // originUrl.searchParams.set("where", "completed = true")
  
  // Select the columns to sync (optional)
  // originUrl.searchParams.set("columns", "id,text,completed")

  const response = await fetch(originUrl)
  const headers = new Headers(response.headers)
  headers.delete("content-encoding")
  headers.delete("content-length")

  return new Response(response.body, {
    status: response.status,
    statusText: response.statusText,
    headers,
  })
}

export const ServerRoute = createServerFileRoute("/api/todos").methods({
  GET: serve,
})
```

## Optimistic Updates with Explicit Transactions

For more advanced use cases, you can create custom actions that can do multiple mutations across collections transactionally. You can use the utility methods to wait for synchronization with different strategies:

### Using Txid Strategy

```typescript
const addTodoAction = createOptimisticAction({
  onMutate: ({ text }) => {
    // optimistically insert with a temporary ID
    const tempId = crypto.randomUUID()
    todosCollection.insert({
      id: tempId,
      text,
      completed: false,
      created_at: new Date(),
    })
    
    // ... mutate other collections
  },
  
  mutationFn: async ({ text }) => {
    const response = await api.todos.create({
      data: { text, completed: false }
    })
    
    // Wait for the specific txid
    await todosCollection.utils.awaitTxId(response.txid)
  }
})
```

### Using Custom Match Function

```typescript
import { isChangeMessage } from '@tanstack/electric-db-collection'

const addTodoAction = createOptimisticAction({
  onMutate: ({ text }) => {
    const tempId = crypto.randomUUID()
    todosCollection.insert({
      id: tempId,
      text,
      completed: false,
      created_at: new Date(),
    })
  },
  
  mutationFn: async ({ text }) => {
    await api.todos.create({
      data: { text, completed: false }
    })
    
    // Wait for matching message
    await todosCollection.utils.awaitMatch(
      (message) => {
        return isChangeMessage(message) && 
               message.headers.operation === 'insert' &&
               message.value.text === text
      }
    )
  }
})
```

## Utility Methods

The collection provides these utility methods via `collection.utils`:

### `awaitTxId(txid, timeout?)`

Manually wait for a specific transaction ID to be synchronized:

```typescript
// Wait for specific txid
await todosCollection.utils.awaitTxId(12345)

// With custom timeout (default is 30 seconds)
await todosCollection.utils.awaitTxId(12345, 10000)
```

This is useful when you need to ensure a mutation has been synchronized before proceeding with other operations.

### `awaitMatch(matchFn, timeout?)`

Manually wait for a custom match function to find a matching message:

```typescript
import { isChangeMessage } from '@tanstack/electric-db-collection'

// Wait for a specific message pattern
await todosCollection.utils.awaitMatch(
  (message) => {
    return isChangeMessage(message) &&
           message.headers.operation === 'insert' &&
           message.value.text === 'New Todo'
  },
  5000 // timeout in ms
)
```

### Helper Functions

The package exports helper functions for use in custom match functions:

- `isChangeMessage(message)`: Check if a message is a data change (insert/update/delete)
- `isControlMessage(message)`: Check if a message is a control message (up-to-date, must-refetch)

```typescript
import { isChangeMessage, isControlMessage } from '@tanstack/electric-db-collection'

// Use in custom match functions
const matchFn = (message) => {
  if (isChangeMessage(message)) {
    return message.headers.operation === 'insert'
  }
  return false
}
```

## Debugging

### Common Issue: awaitTxId Stalls or Times Out

A frequent issue developers encounter is that `awaitTxId` (or the transaction's `isPersisted.promise`) stalls indefinitely, eventually timing out with no error messages. The data persists correctly to the database, but the optimistic mutation never resolves.

**Root Cause:** This happens when the transaction ID (txid) returned from your API doesn't match the actual transaction ID of the mutation in Postgres. This mismatch occurs when you query `pg_current_xact_id()` **outside** the same transaction that performs the mutation.

### Enable Debug Logging

To diagnose txid issues, enable debug logging in your browser console:

```javascript
localStorage.debug = 'ts/db:electric'
```

This will show you when mutations start waiting for txids and when txids arrive from Electric's sync stream.

This is powered by the [debug](https://www.npmjs.com/package/debug) package.

**When txids DON'T match (common bug):**
```
ts/db:electric awaitTxId called with txid 124
ts/db:electric new txids synced from pg [123]
// Stalls forever - 124 never arrives!
```

In this example, the mutation happened in transaction 123, but you queried `pg_current_xact_id()` in a separate transaction (124) that ran after the mutation. The client waits for 124 which will never arrive.

**When txids DO match (correct):**
```
ts/db:electric awaitTxId called with txid 123
ts/db:electric new txids synced from pg [123]
ts/db:electric awaitTxId found match for txid 123
// Resolves immediately!
```

### The Solution: Query txid Inside the Transaction

You **must** call `pg_current_xact_id()` inside the same transaction as your mutation:

**❌ Wrong - txid queried outside transaction:**
```typescript
// DON'T DO THIS
async function createTodo(data) {
  const txid = await generateTxId(sql) // Wrong: separate transaction

  await sql.begin(async (tx) => {
    await tx`INSERT INTO todos ${tx(data)}`
  })

  return { txid } // This txid won't match!
}
```

**✅ Correct - txid queried inside transaction:**
```typescript
// DO THIS
async function createTodo(data) {
  let txid!: Txid

  const result = await sql.begin(async (tx) => {
    // Call generateTxId INSIDE the transaction
    txid = await generateTxId(tx)

    const [todo] = await tx`
      INSERT INTO todos ${tx(data)}
      RETURNING *
    `
    return todo
  })

  return { todo: result, txid } // txid matches the mutation
}

async function generateTxId(tx: any): Promise<Txid> {
  const result = await tx`SELECT pg_current_xact_id()::xid::text as txid`
  const txid = result[0]?.txid

  if (txid === undefined) {
    throw new Error(`Failed to get transaction ID`)
  }

  return parseInt(txid, 10)
}
```

See working examples in:
- `examples/react/todo/src/routes/api/todos.ts`
- `examples/react/todo/src/api/server.ts`


---


---
title: LocalStorage Collection
---

# LocalStorage Collection

LocalStorage collections store small amounts of local-only state that persists across browser sessions and syncs across browser tabs in real-time.

## Overview

The `localStorageCollectionOptions` allows you to create collections that:
- Persist data to localStorage (or sessionStorage)
- Automatically sync across browser tabs using storage events
- Support optimistic updates with automatic rollback on errors
- Store all data under a single localStorage key
- Work with any storage API that matches the localStorage interface

## Installation

LocalStorage collections are included in the core TanStack DB package:

```bash
npm install @tanstack/react-db
```

## Basic Usage

```typescript
import { createCollection } from '@tanstack/react-db'
import { localStorageCollectionOptions } from '@tanstack/react-db'

const userPreferencesCollection = createCollection(
  localStorageCollectionOptions({
    id: 'user-preferences',
    storageKey: 'app-user-prefs',
    getKey: (item) => item.id,
  })
)
```

### Direct Local Mutations

**Important:** LocalStorage collections work differently than server-synced collections. With LocalStorage collections, you **directly mutate state** by calling methods like `collection.insert()`, `collection.update()`, and `collection.delete()` — that's all you need to do. The changes are immediately applied to your local data and automatically persisted to localStorage.

This is different from collections that sync with a server (like Query Collection), where mutation handlers send data to a backend. With LocalStorage collections, everything stays local:

```typescript
// Just call the methods directly - automatically persisted to localStorage
userPreferencesCollection.insert({ id: 'theme', mode: 'dark' })
userPreferencesCollection.update('theme', (draft) => { draft.mode = 'light' })
userPreferencesCollection.delete('theme')
```

## Configuration Options

The `localStorageCollectionOptions` function accepts the following options:

### Required Options

- `id`: Unique identifier for the collection
- `storageKey`: The localStorage key where all collection data is stored
- `getKey`: Function to extract the unique key from an item

### Optional Options

- `schema`: [Standard Schema](https://standardschema.dev) compatible schema (e.g., Zod, Effect) for client-side validation
- `storage`: Custom storage implementation (defaults to `localStorage`). Can be `sessionStorage` or any object with the localStorage API
- `storageEventApi`: Event API for subscribing to storage events (defaults to `window`). Enables custom cross-tab, cross-window, or cross-process synchronization
- `onInsert`: Optional handler function called when items are inserted
- `onUpdate`: Optional handler function called when items are updated
- `onDelete`: Optional handler function called when items are deleted

## Cross-Tab Synchronization

LocalStorage collections automatically sync across browser tabs in real-time:

```typescript
const settingsCollection = createCollection(
  localStorageCollectionOptions({
    id: 'settings',
    storageKey: 'app-settings',
    getKey: (item) => item.id,
  })
)

// Changes in one tab are automatically reflected in all other tabs
// This works automatically via storage events
```

## Using SessionStorage

You can use `sessionStorage` instead of `localStorage` for session-only persistence:

```typescript
const sessionCollection = createCollection(
  localStorageCollectionOptions({
    id: 'session-data',
    storageKey: 'session-key',
    storage: sessionStorage, // Use sessionStorage instead
    getKey: (item) => item.id,
  })
)
```

## Custom Storage Backend

Provide any storage implementation that matches the localStorage API:

```typescript
// Example: Custom storage wrapper with encryption
const encryptedStorage = {
  getItem(key: string) {
    const encrypted = localStorage.getItem(key)
    return encrypted ? decrypt(encrypted) : null
  },
  setItem(key: string, value: string) {
    localStorage.setItem(key, encrypt(value))
  },
  removeItem(key: string) {
    localStorage.removeItem(key)
  },
}

const secureCollection = createCollection(
  localStorageCollectionOptions({
    id: 'secure-data',
    storageKey: 'encrypted-key',
    storage: encryptedStorage,
    getKey: (item) => item.id,
  })
)
```

### Cross-Tab Sync with Custom Storage

The `storageEventApi` option (defaults to `window`) allows the collection to subscribe to storage events for cross-tab synchronization. A custom storage implementation can provide this API to enable custom cross-tab, cross-window, or cross-process sync:

```typescript
// Example: Custom storage event API for cross-process sync
const customStorageEventApi = {
  addEventListener(event: string, handler: (e: StorageEvent) => void) {
    // Custom event subscription logic
    // Could be IPC, WebSocket, or any other mechanism
    myCustomEventBus.on('storage-change', handler)
  },
  removeEventListener(event: string, handler: (e: StorageEvent) => void) {
    myCustomEventBus.off('storage-change', handler)
  },
}

const syncedCollection = createCollection(
  localStorageCollectionOptions({
    id: 'synced-data',
    storageKey: 'data-key',
    storage: customStorage,
    storageEventApi: customStorageEventApi, // Custom event API
    getKey: (item) => item.id,
  })
)
```

This enables synchronization across different contexts beyond just browser tabs, such as:
- Cross-process communication in Electron apps
- WebSocket-based sync across multiple browser windows
- Custom IPC mechanisms in desktop applications

## Mutation Handlers

Mutation handlers are **completely optional**. Data will persist to localStorage whether or not you provide handlers:

```typescript
const preferencesCollection = createCollection(
  localStorageCollectionOptions({
    id: 'preferences',
    storageKey: 'user-prefs',
    getKey: (item) => item.id,
    // Optional: Add custom logic when preferences are updated
    onUpdate: async ({ transaction }) => {
      const { modified } = transaction.mutations[0]
      console.log('Preference updated:', modified)
      // Maybe send analytics or trigger other side effects
    },
  })
)
```

## Manual Transactions

When using LocalStorage collections with manual transactions (created via `createTransaction`), you must call `utils.acceptMutations()` to persist the changes:

```typescript
import { createTransaction } from '@tanstack/react-db'

const localData = createCollection(
  localStorageCollectionOptions({
    id: 'form-draft',
    storageKey: 'draft-data',
    getKey: (item) => item.id,
  })
)

const serverCollection = createCollection(
  queryCollectionOptions({
    queryKey: ['items'],
    queryFn: async () => api.items.getAll(),
    getKey: (item) => item.id,
    onInsert: async ({ transaction }) => {
      await api.items.create(transaction.mutations[0].modified)
    },
  })
)

const tx = createTransaction({
  mutationFn: async ({ transaction }) => {
    // Handle server collection mutations explicitly in mutationFn
    await Promise.all(
      transaction.mutations
        .filter((m) => m.collection === serverCollection)
        .map((m) => api.items.create(m.modified))
    )

    // After server mutations succeed, persist local collection mutations
    localData.utils.acceptMutations(transaction)
  },
})

// Apply mutations to both collections in one transaction
tx.mutate(() => {
  localData.insert({ id: 'draft-1', data: '...' })
  serverCollection.insert({ id: '1', name: 'Item' })
})

await tx.commit()
```

## Complete Example

```typescript
import { createCollection } from '@tanstack/react-db'
import { localStorageCollectionOptions } from '@tanstack/react-db'
import { useLiveQuery } from '@tanstack/react-db'
import { z } from 'zod'

// Define schema
const userPrefsSchema = z.object({
  id: z.string(),
  theme: z.enum(['light', 'dark', 'auto']),
  language: z.string(),
  notifications: z.boolean(),
})

type UserPrefs = z.infer<typeof userPrefsSchema>

// Create collection
export const userPreferencesCollection = createCollection(
  localStorageCollectionOptions({
    id: 'user-preferences',
    storageKey: 'app-user-prefs',
    getKey: (item) => item.id,
    schema: userPrefsSchema,
  })
)

// Use in component
function SettingsPanel() {
  const { data: prefs } = useLiveQuery((q) =>
    q.from({ pref: userPreferencesCollection })
      .where(({ pref }) => pref.id === 'current-user')
  )

  const currentPrefs = prefs[0]

  const updateTheme = (theme: 'light' | 'dark' | 'auto') => {
    if (currentPrefs) {
      userPreferencesCollection.update(currentPrefs.id, (draft) => {
        draft.theme = theme
      })
    } else {
      userPreferencesCollection.insert({
        id: 'current-user',
        theme,
        language: 'en',
        notifications: true,
      })
    }
  }

  return (
    <div>
      <h2>Theme: {currentPrefs?.theme}</h2>
      <button onClick={() => updateTheme('dark')}>Dark Mode</button>
      <button onClick={() => updateTheme('light')}>Light Mode</button>
    </div>
  )
}
```

## Use Cases

LocalStorage collections are perfect for:
- User preferences and settings
- UI state that should persist across sessions
- Form drafts
- Recently viewed items
- User-specific configurations
- Small amounts of cached data

## Learn More

- [Optimistic Mutations](../guides/mutations.md)
- [Live Queries](../guides/live-queries.md)
- [LocalOnly Collection](./local-only-collection.md)


---


---
title: LocalOnly Collection
---

# LocalOnly Collection

LocalOnly collections are designed for in-memory client data or UI state that doesn't need to persist across browser sessions or sync across tabs.

## Overview

The `localOnlyCollectionOptions` allows you to create collections that:
- Store data only in memory (no persistence)
- Support optimistic updates with automatic rollback on errors
- Provide optional initial data
- Work perfectly for temporary UI state and session-only data
- Automatically manage the transition from optimistic to confirmed state

## Installation

LocalOnly collections are included in the core TanStack DB package:

```bash
npm install @tanstack/react-db
```

## Basic Usage

```typescript
import { createCollection } from '@tanstack/react-db'
import { localOnlyCollectionOptions } from '@tanstack/react-db'

const uiStateCollection = createCollection(
  localOnlyCollectionOptions({
    id: 'ui-state',
    getKey: (item) => item.id,
  })
)
```

### Direct Local Mutations

**Important:** LocalOnly collections work differently than server-synced collections. With LocalOnly collections, you **directly mutate state** by calling methods like `collection.insert()`, `collection.update()`, and `collection.delete()` — that's all you need to do. The changes are immediately applied to your local in-memory data.

This is different from collections that sync with a server (like Query Collection), where mutation handlers send data to a backend. With LocalOnly collections, everything stays local:

```typescript
// Just call the methods directly - no server sync involved
uiStateCollection.insert({ id: 'theme', mode: 'dark' })
uiStateCollection.update('theme', (draft) => { draft.mode = 'light' })
uiStateCollection.delete('theme')
```

## Configuration Options

The `localOnlyCollectionOptions` function accepts the following options:

### Required Options

- `id`: Unique identifier for the collection
- `getKey`: Function to extract the unique key from an item

### Optional Options

- `schema`: [Standard Schema](https://standardschema.dev) compatible schema (e.g., Zod, Effect) for client-side validation
- `initialData`: Array of items to populate the collection with on creation
- `onInsert`: Optional handler function called before confirming inserts
- `onUpdate`: Optional handler function called before confirming updates
- `onDelete`: Optional handler function called before confirming deletes

## Initial Data

Populate the collection with initial data on creation:

```typescript
const uiStateCollection = createCollection(
  localOnlyCollectionOptions({
    id: 'ui-state',
    getKey: (item) => item.id,
    initialData: [
      { id: 'sidebar', isOpen: false },
      { id: 'theme', mode: 'light' },
      { id: 'modal', visible: false },
    ],
  })
)
```

## Mutation Handlers

Mutation handlers are **completely optional**. When provided, they are called before the optimistic state is confirmed:

```typescript
const tempDataCollection = createCollection(
  localOnlyCollectionOptions({
    id: 'temp-data',
    getKey: (item) => item.id,
    onInsert: async ({ transaction }) => {
      // Custom logic before confirming the insert
      console.log('Inserting:', transaction.mutations[0].modified)
    },
    onUpdate: async ({ transaction }) => {
      // Custom logic before confirming the update
      const { original, modified } = transaction.mutations[0]
      console.log('Updating from', original, 'to', modified)
    },
    onDelete: async ({ transaction }) => {
      // Custom logic before confirming the delete
      console.log('Deleting:', transaction.mutations[0].original)
    },
  })
)
```

## Manual Transactions

When using LocalOnly collections with manual transactions (created via `createTransaction`), you must call `utils.acceptMutations()` to persist the changes:

```typescript
import { createTransaction } from '@tanstack/react-db'

const localData = createCollection(
  localOnlyCollectionOptions({
    id: 'form-draft',
    getKey: (item) => item.id,
  })
)

const serverCollection = createCollection(
  queryCollectionOptions({
    queryKey: ['items'],
    queryFn: async () => api.items.getAll(),
    getKey: (item) => item.id,
    onInsert: async ({ transaction }) => {
      await api.items.create(transaction.mutations[0].modified)
    },
  })
)

const tx = createTransaction({
  mutationFn: async ({ transaction }) => {
    // Handle server collection mutations explicitly in mutationFn
    await Promise.all(
      transaction.mutations
        .filter((m) => m.collection === serverCollection)
        .map((m) => api.items.create(m.modified))
    )

    // After server mutations succeed, accept local collection mutations
    localData.utils.acceptMutations(transaction)
  },
})

// Apply mutations to both collections in one transaction
tx.mutate(() => {
  localData.insert({ id: 'draft-1', data: '...' })
  serverCollection.insert({ id: '1', name: 'Item' })
})

await tx.commit()
```

## Complete Example: Modal State Management

```typescript
import { createCollection } from '@tanstack/react-db'
import { localOnlyCollectionOptions } from '@tanstack/react-db'
import { useLiveQuery } from '@tanstack/react-db'
import { z } from 'zod'

// Define schema
const modalStateSchema = z.object({
  id: z.string(),
  isOpen: z.boolean(),
  data: z.any().optional(),
})

type ModalState = z.infer<typeof modalStateSchema>

// Create collection
export const modalStateCollection = createCollection(
  localOnlyCollectionOptions({
    id: 'modal-state',
    getKey: (item) => item.id,
    schema: modalStateSchema,
    initialData: [
      { id: 'user-profile', isOpen: false },
      { id: 'settings', isOpen: false },
      { id: 'confirm-delete', isOpen: false },
    ],
  })
)

// Use in component
function UserProfileModal() {
  const { data: modals } = useLiveQuery((q) =>
    q.from({ modal: modalStateCollection })
      .where(({ modal }) => modal.id === 'user-profile')
  )

  const modalState = modals[0]

  const openModal = (data?: any) => {
    modalStateCollection.update('user-profile', (draft) => {
      draft.isOpen = true
      draft.data = data
    })
  }

  const closeModal = () => {
    modalStateCollection.update('user-profile', (draft) => {
      draft.isOpen = false
      draft.data = undefined
    })
  }

  if (!modalState?.isOpen) return null

  return (
    <div className="modal">
      <h2>User Profile</h2>
      <pre>{JSON.stringify(modalState.data, null, 2)}</pre>
      <button onClick={closeModal}>Close</button>
    </div>
  )
}
```

## Complete Example: Form Draft State

```typescript
import { createCollection } from '@tanstack/react-db'
import { localOnlyCollectionOptions } from '@tanstack/react-db'
import { useLiveQuery } from '@tanstack/react-db'

type FormDraft = {
  id: string
  formData: Record<string, any>
  lastModified: Date
}

// Create collection for form drafts
export const formDraftsCollection = createCollection(
  localOnlyCollectionOptions({
    id: 'form-drafts',
    getKey: (item) => item.id,
  })
)

// Use in component
function CreatePostForm() {
  const { data: drafts } = useLiveQuery((q) =>
    q.from({ draft: formDraftsCollection })
      .where(({ draft }) => draft.id === 'new-post')
  )

  const currentDraft = drafts[0]

  const updateDraft = (field: string, value: any) => {
    if (currentDraft) {
      formDraftsCollection.update('new-post', (draft) => {
        draft.formData[field] = value
        draft.lastModified = new Date()
      })
    } else {
      formDraftsCollection.insert({
        id: 'new-post',
        formData: { [field]: value },
        lastModified: new Date(),
      })
    }
  }

  const clearDraft = () => {
    if (currentDraft) {
      formDraftsCollection.delete('new-post')
    }
  }

  const submitForm = async () => {
    if (!currentDraft) return

    await api.posts.create(currentDraft.formData)
    clearDraft()
  }

  return (
    <form onSubmit={(e) => { e.preventDefault(); submitForm() }}>
      <input
        value={currentDraft?.formData.title || ''}
        onChange={(e) => updateDraft('title', e.target.value)}
      />
      <button type="submit">Publish</button>
      <button type="button" onClick={clearDraft}>Clear Draft</button>
    </form>
  )
}
```

## Use Cases

LocalOnly collections are perfect for:
- Temporary UI state (modals, sidebars, tooltips)
- Form draft data during the current session
- Client-side computed or derived data
- Wizard/multi-step form state
- Temporary filters or search state
- In-memory caches

## Comparison with LocalStorageCollection

| Feature | LocalOnly | LocalStorage |
|---------|-----------|--------------|
| Persistence | None (in-memory only) | localStorage |
| Cross-tab sync | No | Yes |
| Survives page reload | No | Yes |
| Performance | Fastest | Fast |
| Size limits | Memory limits | ~5-10MB |
| Best for | Temporary UI state | User preferences |

## Learn More

- [Optimistic Mutations](../guides/mutations.md)
- [Live Queries](../guides/live-queries.md)
- [LocalStorage Collection](./local-storage-collection.md)


---


---
title: PowerSync Collection
---

# PowerSync Collection

PowerSync collections provide seamless integration between TanStack DB and [PowerSync](https://powersync.com), enabling automatic synchronization between your in-memory TanStack DB collections and PowerSync's SQLite database. This gives you offline-ready persistence, real-time sync capabilities, and powerful conflict resolution.

## Overview

The `@tanstack/powersync-db-collection` package allows you to create collections that:

- Automatically mirror the state of an underlying PowerSync SQLite database
- Reactively update when PowerSync records change
- Support optimistic mutations with rollback on error
- Provide persistence handlers to keep PowerSync in sync with TanStack DB transactions
- Use PowerSync's efficient SQLite-based storage engine
- Work with PowerSync's real-time sync features for offline-first scenarios
- Leverage PowerSync's built-in conflict resolution and data consistency guarantees
- Enable real-time synchronization with PostgreSQL, MongoDB and MySQL backends

## 1. Installation

Install the PowerSync collection package along with your preferred framework integration.
PowerSync currently works with Web, React Native and Node.js. The examples below use the Web SDK.
See the PowerSync quickstart [docs](https://docs.powersync.com/installation/quickstart-guide) for more details.

```bash
npm install @tanstack/powersync-db-collection @powersync/web @journeyapps/wa-sqlite
```

### 2. Create a PowerSync Database and Schema

```ts
import { Schema, Table, column } from "@powersync/web"

// Define your schema
const APP_SCHEMA = new Schema({
  documents: new Table({
    name: column.text,
    author: column.text,
    created_at: column.text,
    archived: column.integer,
  }),
})

// Initialize PowerSync database
const db = new PowerSyncDatabase({
  database: {
    dbFilename: "app.sqlite",
  },
  schema: APP_SCHEMA,
})
```

### 3. (optional) Configure Sync with a Backend

```ts
import {
  AbstractPowerSyncDatabase,
  PowerSyncBackendConnector,
  PowerSyncCredentials,
} from "@powersync/web"

// TODO implement your logic here
class Connector implements PowerSyncBackendConnector {
  fetchCredentials: () => Promise<PowerSyncCredentials | null>

  /** Upload local changes to the app backend.
   *
   * Use {@link AbstractPowerSyncDatabase.getCrudBatch} to get a batch of changes to upload.
   *
   * Any thrown errors will result in a retry after the configured wait period (default: 5 seconds).
   */
  uploadData: (database: AbstractPowerSyncDatabase) => Promise<void>
}

// Configure the client to connect to a PowerSync service and your backend
db.connect(new Connector())
```

### 4. Create a TanStack DB Collection

There are two main ways to create a collection: using type inference or using schema validation. Type inference will infer collection types from the underlying PowerSync SQLite tables. Schema validation can be used for additional input/output validations and type transforms.

#### Option 1: Using Table Type Inference

The collection types are automatically inferred from the PowerSync schema table definition. The table is used to construct a default standard schema validator which is used internally to validate collection operations.

Collection mutations accept SQLite types and queries report data with SQLite types.

```ts
import { createCollection } from "@tanstack/react-db"
import { powerSyncCollectionOptions } from "@tanstack/powersync-db-collection"

const documentsCollection = createCollection(
  powerSyncCollectionOptions({
    database: db,
    table: APP_SCHEMA.props.documents,
  })
)

/** Note: The types for input and output are defined as this */
// Used for mutations like `insert` or `update`
type DocumentCollectionInput = {
  id: string
  name: string | null
  author: string | null
  created_at: string | null // SQLite TEXT
  archived: number | null // SQLite integer
}
// The type of query/data results
type DocumentCollectionOutput = DocumentCollectionInput
```

The standard PowerSync SQLite types map to these TypeScript types:

| PowerSync Column Type | TypeScript Type  | Description                                                          |
| --------------------- | ---------------- | -------------------------------------------------------------------- |
| `column.text`         | `string \| null` | Text values, commonly used for strings, JSON, dates (as ISO strings) |
| `column.integer`      | `number \| null` | Integer values, also used for booleans (0/1)                         |
| `column.real`         | `number \| null` | Floating point numbers                                               |

Note: All PowerSync column types are nullable by default.

#### Option 2: SQLite Types with Schema Validation

Additional validations for collection mutations can be performed with a custom schema. The Schema below asserts that
the `name`, `author` and `created_at` fields are required as input. `name` also has an additional string length check.

Note: The input and output types specified in this example still satisfy the underlying SQLite types. An additional `deserializationSchema` is required if the typing differs. See the examples below for more details.

The application logic (including the backend) should enforce that all incoming synced data passes validation with the `schema`. Failing to validate data will result in inconsistency of the collection data. This is a fatal error! An `onDeserializationError` handler must be provided to react to this case.

```ts
import { createCollection } from "@tanstack/react-db"
import { powerSyncCollectionOptions } from "@tanstack/powersync-db-collection"
import { z } from "zod"

// Schema validates SQLite types but adds constraints
const schema = z.object({
  id: z.string(),
  name: z.string().min(3, { message: "Should be at least 3 characters" }),
  author: z.string(),
  created_at: z.string(), // SQLite TEXT for dates
  archived: z.number(),
})

const documentsCollection = createCollection(
  powerSyncCollectionOptions({
    database: db,
    table: APP_SCHEMA.props.documents,
    schema,
    onDeserializationError: (error) => {
      // Present fatal error
    },
  })
)

/** Note: The types for input and output are defined as this */
// Used for mutations like `insert` or `update`
type DocumentCollectionInput = {
  id: string
  name: string
  author: string
  created_at: string // SQLite TEXT
  archived: number // SQLite integer
}
// The type of query/data results
type DocumentCollectionOutput = DocumentCollectionInput
```

#### Option 3: Transform SQLite Input Types to Rich Output Types

You can transform SQLite types to richer types (like Date objects) while keeping SQLite-compatible input types:

Note: The Transformed types are provided by TanStackDB to the PowerSync SQLite persister. These types need to be serialized in
order to be persisted to SQLite. Most types are converted by default. For custom types, override the serialization by providing a
`serializer` param.

The example below uses `nullable` columns, this is not a requirement.

The application logic (including the backend) should enforce that all incoming synced data passes validation with the `schema`. Failing to validate data will result in inconsistency of the collection data. This is a fatal error! An `onDeserializationError` handler must be provided to react to this case.

```ts
const schema = z.object({
  id: z.string(),
  name: z.string().nullable(),
  created_at: z
    .string()
    .nullable()
    .transform((val) => (val ? new Date(val) : null)), // Transform SQLite TEXT to Date
  archived: z
    .number()
    .nullable()
    .transform((val) => (val != null ? val > 0 : null)), // Transform SQLite INTEGER to boolean
})

const documentsCollection = createCollection(
  powerSyncCollectionOptions({
    database: db,
    table: APP_SCHEMA.props.documents,
    schema,
    onDeserializationError: (error) => {
      // Present fatal error
    },
    // Optional: custom column serialization
    serializer: {
      // Dates are serialized by default, this is just an example
      created_at: (value) => (value ? value.toISOString() : null),
    },
  })
)

/** Note: The types for input and output are defined as this */
// Used for mutations like `insert` or `update`
type DocumentCollectionInput = {
  id: string
  name: string | null
  author: string | null
  created_at: string | null // SQLite TEXT
  archived: number | null
}
// The type of query/data results
type DocumentCollectionOutput = {
  id: string
  name: string | null
  author: string | null
  created_at: Date | null // JS Date instance
  archived: boolean | null // JS boolean
}
```

#### Option 4: Custom Input/Output Types with Deserialization

The input and output types can be completely decoupled from the internal SQLite types. This can be used to accept rich values for input mutations.
We require an additional `deserializationSchema` in order to validate and transform incoming synced (SQLite) updates. This schema should convert the incoming SQLite update to the output type.

The application logic (including the backend) should enforce that all incoming synced data passes validation with the `deserializationSchema`. Failing to validate data will result in inconsistency of the collection data. This is a fatal error! An `onDeserializationError` handler must be provided to react to this case.

```ts
// Our input/output types use Date and boolean
const schema = z.object({
  id: z.string(),
  name: z.string(),
  author: z.string(),
  created_at: z.date(), // Accept Date objects as input
  archived: z.boolean(), // Accept Booleans as input
})

// Schema to transform from SQLite types to our output types
const deserializationSchema = z.object({
  id: z.string(),
  name: z.string(),
  author: z.string(),
  created_at: z
    .string()
    .transform((val) => (new Date(val))), // SQLite TEXT to Date
  archived: z
    .number()
    .transform((val) => (val > 0), // SQLite INTEGER to Boolean
})

const documentsCollection = createCollection(
  powerSyncCollectionOptions({
    database: db,
    table: APP_SCHEMA.props.documents,
    schema,
    deserializationSchema,
    onDeserializationError: (error) => {
      // Present fatal error
    },
  })
)

/** Note: The types for input and output are defined as this */
// Used for mutations like `insert` or `update`
type DocumentCollectionInput = {
  id: string
  name: string
  author: string
  created_at: Date
  archived: boolean
}
// The type of query/data results
type DocumentCollectionOutput = DocumentCollectionInput
```

## Features

### Offline-First

PowerSync collections are offline-first by default. All data is stored locally in a SQLite database, allowing your app to work without an internet connection. Changes are automatically synced when connectivity is restored.

### Real-Time Sync

When connected to a PowerSync backend, changes are automatically synchronized in real-time across all connected clients. The sync process handles:

- Bi-directional sync with the server
- Conflict resolution
- Queue management for offline changes
- Automatic retries on connection loss

### Working with Rich JavaScript Types

PowerSync collections support rich JavaScript types like `Date`, `Boolean`, and custom objects while maintaining SQLite compatibility. The collection handles serialization and deserialization automatically:

```typescript
import { z } from "zod"
import { Schema, Table, column } from "@powersync/web"
import { createCollection } from "@tanstack/react-db"
import { powerSyncCollectionOptions } from "@tanstack/powersync-db-collection"

// Define PowerSync SQLite schema
const APP_SCHEMA = new Schema({
  tasks: new Table({
    title: column.text,
    due_date: column.text, // Stored as ISO string in SQLite
    completed: column.integer, // Stored as 0/1 in SQLite
    metadata: column.text, // Stored as JSON string in SQLite
  }),
})

// Define rich types schema
const taskSchema = z.object({
  id: z.string(),
  title: z.string().nullable(),
  due_date: z
    .string()
    .nullable()
    .transform((val) => (val ? new Date(val) : null)), // Convert to Date
  completed: z
    .number()
    .nullable()
    .transform((val) => (val != null ? val > 0 : null)), // Convert to boolean
  metadata: z
    .string()
    .nullable()
    .transform((val) => (val ? JSON.parse(val) : null)), // Parse JSON
})

// Create collection with rich types
const tasksCollection = createCollection(
  powerSyncCollectionOptions({
    database: db,
    table: APP_SCHEMA.props.tasks,
    schema: taskSchema,
  })
)

// Work with rich types in your code
await tasksCollection.insert({
  id: crypto.randomUUID(),
  title: "Review PR",
  due_date: "2025-10-30T10:00:00Z", // String input is automatically converted to Date
  completed: 0, // Number input is automatically converted to boolean
  metadata: JSON.stringify({ priority: "high" }),
})

// Query returns rich types
const task = tasksCollection.get("task-1")
console.log(task.due_date instanceof Date) // true
console.log(typeof task.completed) // "boolean"
console.log(task.metadata.priority) // "high"
```

### Type Safety with Rich Types

The collection maintains type safety throughout:

```typescript
type TaskInput = {
  id: string
  title: string | null
  due_date: string | null // Accept ISO string for mutations
  completed: number | null // Accept 0/1 for mutations
  metadata: string | null // Accept JSON string for mutations
}

type TaskOutput = {
  id: string
  title: string | null
  due_date: Date | null // Get Date object in queries
  completed: boolean | null // Get boolean in queries
  metadata: {
    priority: string
    [key: string]: any
  } | null
}

// TypeScript enforces correct types:
tasksCollection.insert({
  due_date: new Date(), // Error: Type 'Date' is not assignable to type 'string'
})

const task = tasksCollection.get("task-1")
task.due_date.getTime() // OK - TypeScript knows this is a Date
```

### Optimistic Updates

Updates to the collection are applied optimistically to the local state first, then synchronized with PowerSync and the backend. If an error occurs during sync, the changes are automatically rolled back.

## Configuration Options

The `powerSyncCollectionOptions` function accepts the following options:

```ts
interface PowerSyncCollectionConfig<TTable extends Table, TSchema> {
  // Required options
  database: PowerSyncDatabase
  table: Table

  // Schema validation and type transformation
  schema?: StandardSchemaV1
  deserializationSchema?: StandardSchemaV1 // Required for custom input types
  onDeserializationError?: (error: StandardSchemaV1.FailureResult) => void // Required for custom input types

  // Optional Custom serialization
  serializer?: {
    [Key in keyof TOutput]?: (value: TOutput[Key]) => SQLiteCompatibleType
  }

  // Performance tuning
  syncBatchSize?: number // Control batch size for initial sync, defaults to 1000
}
```

## Advanced Transactions

When you need more control over transaction handling, such as batching multiple operations or handling complex transaction scenarios, you can use PowerSync's transaction system directly with TanStack DB transactions.

```ts
import { createTransaction } from "@tanstack/react-db"
import { PowerSyncTransactor } from "@tanstack/powersync-db-collection"

// Create a transaction that won't auto-commit
const batchTx = createTransaction({
  autoCommit: false,
  mutationFn: async ({ transaction }) => {
    // Use PowerSyncTransactor to apply the transaction to PowerSync
    await new PowerSyncTransactor({ database: db }).applyTransaction(
      transaction
    )
  },
})

// Perform multiple operations in the transaction
batchTx.mutate(() => {
  // Add multiple documents in a single transaction
  for (let i = 0; i < 5; i++) {
    documentsCollection.insert({
      id: crypto.randomUUID(),
      name: `Document ${i}`,
      content: `Content ${i}`,
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString(),
    })
  }
})

// Commit the transaction
await batchTx.commit()

// Wait for the changes to be persisted
await batchTx.isPersisted.promise
```

This approach allows you to:

- Batch multiple operations into a single transaction
- Control when the transaction is committed
- Ensure all operations are atomic
- Wait for persistence confirmation
- Handle complex transaction scenarios


---


---
title: RxDB Collection
---

# RxDB Collection

RxDB collections provide seamless integration between TanStack DB and [RxDB](https://rxdb.info), enabling automatic synchronization between your in-memory TanStack DB collections and RxDB's local-first database. Giving you offline-ready persistence, and powerful sync capabilities with a wide range of backends.


## Overview

The `@tanstack/rxdb-db-collection` package allows you to create collections that:
- Automatically mirror the state of an underlying RxDB collection
- Reactively update when RxDB documents change
- Support optimistic mutations with rollback on error
- Provide persistence handlers to keep RxDB in sync with TanStack DB transactions
- Sync across browser tabs - changes in one tab are reflected in RxDB and TanStack DB collections in all tabs
- Use one of RxDB's [storage engines](https://rxdb.info/rx-storage.html).
- Work with RxDB's [replication features](https://rxdb.info/replication.html) for offline-first and sync scenarios
- Leverage RxDB's [replication plugins](https://rxdb.info/replication.html) to sync with CouchDB, MongoDB, Supabase, REST APIs, GraphQL, WebRTC (P2P) and more.


## 1. Installation

Install the RXDB collection packages along with your preferred framework integration.

```bash
npm install @tanstack/rxdb-db-collection rxdb @tanstack/react-db
```


### 2. Create an RxDatabase and RxCollection

```ts
import { createRxDatabase, addRxPlugin } from 'rxdb/plugins/core'

/**
 * Here we use the localstorage based storage for RxDB.
 * RxDB has a wide range of storages based on Dexie.js, IndexedDB, SQLite and more.
 */
import { getRxStorageLocalstorage } from 'rxdb/plugins/storage-localstorage'

// add json-schema validation (optional)
import { wrappedValidateAjvStorage } from 'rxdb/plugins/validate-ajv';

// Enable dev mode (optional, recommended during development)
import { RxDBDevModePlugin } from 'rxdb/plugins/dev-mode'
addRxPlugin(RxDBDevModePlugin)

type Todo = { id: string; text: string; completed: boolean }

const db = await createRxDatabase({
  name: 'my-todos',
  storage: wrappedValidateAjvStorage({
    storage: getRxStorageLocalstorage()
  })
})

await db.addCollections({
  todos: {
    schema: {
      title: 'todos',
      version: 0,
      type: 'object',
      primaryKey: 'id',
      properties: {
        id: { type: 'string', maxLength: 100 },
        text: { type: 'string' },
        completed: { type: 'boolean' },
      },
      required: ['id', 'text', 'completed'],
    },
  },
})
```


### 3. (optional) sync with a backend
```ts
import { replicateRxCollection } from 'rxdb/plugins/replication'
const replicationState = replicateRxCollection({
  collection: db.todos,
  pull: { handler: myPullHandler },
  push: { handler: myPushHandler },
})
```

### 4. Wrap the RxDB collection with TanStack DB

```ts
import { createCollection } from '@tanstack/react-db'
import { rxdbCollectionOptions } from '@tanstack/rxdb-db-collection'

const todosCollection = createCollection(
  rxdbCollectionOptions({
    rxCollection: myDatabase.todos,
    startSync: true, // start ingesting RxDB data immediately
  })
)
```


Now `todosCollection` is a reactive TanStack DB collection driven by RxDB:

- Writes via `todosCollection.insert/update/delete` persist to RxDB.
- Direct writes in RxDB (or via replication) flow into the TanStack collection via change streams.



## Configuration Options

The `rxdbCollectionOptions` function accepts the following options:

### Required

- `rxCollection`: The underlying [RxDB collection](https://rxdb.info/rx-collection.html)

### Optional

- `id`: Unique identifier for the collection
- `schema`: Schema for validating items. RxDB already has schema validation but having additional validation on the TanStack DB side can help to unify error handling between different tanstack collections.
- `startSync`: Whether to start syncing immediately (default: true)
- `onInsert, onUpdate, onDelete`: Override default persistence handlers. By default, TanStack DB writes are persisted to RxDB using bulkUpsert, patch, and bulkRemove.
- `syncBatchSize`: The maximum number of documents fetched per batch during the initial sync from RxDB into TanStack DB (default: 1000). Larger values reduce round trips but use more memory; smaller values are lighter but may increase query calls. Note that this only affects the initial sync. Ongoing live updates are streamed one by one via RxDB's change feed.



## Syncing with Backends

Replication and sync in RxDB run independently of TanStack DB. You set up replication directly on your RxCollection using RxDB's replication plugins (for CouchDB, GraphQL, WebRTC, REST APIs, etc.).

When replication runs, it pulls and pushes changes to the backend and applies them to the RxDB collection. Since the TanStack DB integration subscribes to the RxDB change stream, any changes applied by replication are automatically reflected in your TanStack DB collection.

This separation of concerns means you configure replication entirely in RxDB, and TanStack DB automatically benefits: your TanStack collections always stay up to date with whatever sync strategy you choose.


---


---
title: TrailBase Collection
---

# TrailBase Collection

TrailBase collections provide seamless integration between TanStack DB and [TrailBase](https://trailbase.io), enabling real-time data synchronization with TrailBase's self-hosted application backend.

## Overview

[TrailBase](https://trailbase.io) is an easy-to-self-host, single-executable application backend with built-in SQLite, a V8 JS runtime, auth, admin UIs and sync functionality.

The `@tanstack/trailbase-db-collection` package allows you to create collections that:
- Automatically sync data from TrailBase Record APIs
- Support real-time subscriptions when `enable_subscriptions` is enabled
- Handle optimistic updates with automatic rollback on errors
- Provide parse/serialize functions for data transformation

## Installation

```bash
npm install @tanstack/trailbase-db-collection @tanstack/react-db trailbase
```

## Basic Usage

```typescript
import { createCollection } from '@tanstack/react-db'
import { trailBaseCollectionOptions } from '@tanstack/trailbase-db-collection'
import { initClient } from 'trailbase'

const trailBaseClient = initClient(`https://your-trailbase-instance.com`)

const todosCollection = createCollection(
  trailBaseCollectionOptions({
    id: 'todos',
    recordApi: trailBaseClient.records('todos'),
    getKey: (item) => item.id,
  })
)
```

## Configuration Options

The `trailBaseCollectionOptions` function accepts the following options:

### Required Options

- `id`: Unique identifier for the collection
- `recordApi`: TrailBase Record API instance created via `trailBaseClient.records()`
- `getKey`: Function to extract the unique key from an item

### Optional Options

- `schema`: [Standard Schema](https://standardschema.dev) compatible schema (e.g., Zod, Effect) for client-side validation
- `parse`: Object mapping field names to parsing functions that transform data coming from TrailBase
- `serialize`: Object mapping field names to serialization functions that transform data going to TrailBase
- `onInsert`: Handler function called when items are inserted
- `onUpdate`: Handler function called when items are updated
- `onDelete`: Handler function called when items are deleted

## Data Transformation

TrailBase uses different data formats for storage (e.g., Unix timestamps). Use `parse` and `serialize` to handle these transformations:

```typescript
type SelectTodo = {
  id: string
  text: string
  created_at: number // Unix timestamp from TrailBase
  completed: boolean
}

type Todo = {
  id: string
  text: string
  created_at: Date // JavaScript Date for app usage
  completed: boolean
}

const todosCollection = createCollection<SelectTodo, Todo>(
  trailBaseCollectionOptions({
    id: 'todos',
    recordApi: trailBaseClient.records('todos'),
    getKey: (item) => item.id,
    schema: todoSchema,
    // Transform TrailBase data to application format
    parse: {
      created_at: (ts) => new Date(ts * 1000),
    },
    // Transform application data to TrailBase format
    serialize: {
      created_at: (date) => Math.floor(date.valueOf() / 1000),
    },
  })
)
```

## Real-time Subscriptions

TrailBase supports real-time subscriptions when enabled on the server. The collection automatically subscribes to changes and updates in real-time:

```typescript
const todosCollection = createCollection(
  trailBaseCollectionOptions({
    id: 'todos',
    recordApi: trailBaseClient.records('todos'),
    getKey: (item) => item.id,
    // Real-time updates work automatically when
    // enable_subscriptions is set in TrailBase config
  })
)

// Changes from other clients will automatically update
// the collection in real-time
```

## Mutation Handlers

Handle inserts, updates, and deletes by providing mutation handlers:

```typescript
const todosCollection = createCollection(
  trailBaseCollectionOptions({
    id: 'todos',
    recordApi: trailBaseClient.records('todos'),
    getKey: (item) => item.id,
    onInsert: async ({ transaction }) => {
      const newTodo = transaction.mutations[0].modified
      // TrailBase handles the persistence automatically
      // Add custom logic here if needed
    },
    onUpdate: async ({ transaction }) => {
      const { original, modified } = transaction.mutations[0]
      // TrailBase handles the persistence automatically
      // Add custom logic here if needed
    },
    onDelete: async ({ transaction }) => {
      const deletedTodo = transaction.mutations[0].original
      // TrailBase handles the persistence automatically
      // Add custom logic here if needed
    },
  })
)
```

## Complete Example

```typescript
import { createCollection } from '@tanstack/react-db'
import { trailBaseCollectionOptions } from '@tanstack/trailbase-db-collection'
import { initClient } from 'trailbase'
import { z } from 'zod'

const trailBaseClient = initClient(`https://your-trailbase-instance.com`)

// Define schema
const todoSchema = z.object({
  id: z.string(),
  text: z.string(),
  completed: z.boolean(),
  created_at: z.date(),
})

type SelectTodo = {
  id: string
  text: string
  completed: boolean
  created_at: number
}

type Todo = z.infer<typeof todoSchema>

// Create collection
export const todosCollection = createCollection<SelectTodo, Todo>(
  trailBaseCollectionOptions({
    id: 'todos',
    recordApi: trailBaseClient.records('todos'),
    getKey: (item) => item.id,
    schema: todoSchema,
    parse: {
      created_at: (ts) => new Date(ts * 1000),
    },
    serialize: {
      created_at: (date) => Math.floor(date.valueOf() / 1000),
    },
    onInsert: async ({ transaction }) => {
      const newTodo = transaction.mutations[0].modified
      console.log('Todo created:', newTodo)
    },
  })
)

// Use in component
function TodoList() {
  const { data: todos } = useLiveQuery((q) =>
    q.from({ todo: todosCollection })
      .where(({ todo }) => !todo.completed)
      .orderBy(({ todo }) => todo.created_at, 'desc')
  )

  const addTodo = (text: string) => {
    todosCollection.insert({
      id: crypto.randomUUID(),
      text,
      completed: false,
      created_at: new Date(),
    })
  }

  return (
    <div>
      {todos.map((todo) => (
        <div key={todo.id}>{todo.text}</div>
      ))}
    </div>
  )
}
```

## Learn More

- [TrailBase Documentation](https://trailbase.io/documentation/)
- [TrailBase Record APIs](https://trailbase.io/documentation/apis_record/)
- [Optimistic Mutations](../guides/mutations.md)
- [Live Queries](../guides/live-queries.md)


---

# GUIDES

---


---
title: Live Queries
id: live-queries
---

# TanStack DB Live Queries

TanStack DB provides a powerful, type-safe query system that allows you to fetch, filter, transform, and aggregate data from collections using a SQL-like fluent API. All queries are **live** by default, meaning they automatically update when the underlying data changes.

The query system is built around an API similar to SQL query builders like Kysely or Drizzle where you chain methods together to compose your query. The query builder doesn't perform operations in the order of method calls - instead, it composes your query into an optimal incremental pipeline that gets compiled and executed efficiently. Each method returns a new query builder, allowing you to chain operations together.

Live queries resolve to collections that automatically update when their underlying data changes. You can subscribe to changes, iterate over results, and use all the standard collection methods.

```ts
import { createCollection, liveQueryCollectionOptions, eq } from '@tanstack/db'

const activeUsers = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
      .select(({ user }) => ({
        id: user.id,
        name: user.name,
        email: user.email,
      }))
}))
```

The result types are automatically inferred from your query structure, providing full TypeScript support. When you use a `select` clause, the result type matches your projection. Without `select`, you get the full schema with proper join optionality.

## Table of Contents

- [Creating Live Query Collections](#creating-live-query-collections)
- [From Clause](#from-clause)
- [Where Clauses](#where-clauses)
- [Select Projections](#select)
- [Joins](#joins)
- [Subqueries](#subqueries)
- [groupBy and Aggregations](#groupby-and-aggregations)
- [findOne](#findone)
- [Distinct](#distinct)
- [Order By, Limit, and Offset](#order-by-limit-and-offset)
- [Composable Queries](#composable-queries)
- [Expression Functions Reference](#expression-functions-reference)
- [Functional Variants](#functional-variants)

## Creating Live Query Collections

To create a live query collection, you can use `liveQueryCollectionOptions` with `createCollection`, or use the convenience function `createLiveQueryCollection`.

### Using liveQueryCollectionOptions

The fundamental way to create a live query is using `liveQueryCollectionOptions` with `createCollection`:

```ts
import { createCollection, liveQueryCollectionOptions, eq } from '@tanstack/db'

const activeUsers = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
      .select(({ user }) => ({
        id: user.id,
        name: user.name,
      }))
}))
```

### Configuration Options

For more control, you can specify additional options:

```ts
const activeUsers = createCollection(liveQueryCollectionOptions({
  id: 'active-users', // Optional: auto-generated if not provided
  query: (q) =>
    q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
      .select(({ user }) => ({
        id: user.id,
        name: user.name,
      })),
  getKey: (user) => user.id, // Optional: uses stream key if not provided
  startSync: true, // Optional: starts sync immediately
}))
```
| Option | Type | Description |
|--------|------|-------------|
| `id` | `string` (optional) | An optional unique identifier for the live query. If not provided, it will be auto-generated. This is useful for debugging and logging. |
| `query` | `QueryBuilder` or function | The query definition, this is either a `Query` instance or a function that returns a `Query` instance. |
| `getKey` | `(item) => string \| number` (optional) | A function that extracts a unique key from each row. If not provided, the stream's internal key will be used. For simple cases this is the key from the parent collection, but in the case of joins, the auto-generated key will be a composite of the parent keys. Using `getKey` is useful when you want to use a specific key from a parent collection for the resulting collection. |
| `schema` | `Schema` (optional) | Optional schema for validation |
| `startSync` | `boolean` (optional) | Whether to start syncing immediately. Defaults to `true`. |
| `gcTime` | `number` (optional) | Garbage collection time in milliseconds. Defaults to `5000` (5 seconds). |

### Convenience Function

For simpler cases, you can use `createLiveQueryCollection` as a shortcut:

```ts
import { createLiveQueryCollection, eq } from '@tanstack/db'

const activeUsers = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .where(({ user }) => eq(user.active, true))
    .select(({ user }) => ({
      id: user.id,
      name: user.name,
    }))
)
```

### Using with Frameworks

In React, you can use the `useLiveQuery` hook:

```tsx
import { useLiveQuery } from '@tanstack/react-db'

function UserList() {
  const activeUsers = useLiveQuery((q) =>
    q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
  )

  return (
    <ul>
      {activeUsers.map(user => (
        <li key={user.id}>{user.name}</li>
      ))}
    </ul>
  )
}
```

In Angular, you can use the `injectLiveQuery` function:

```typescript
import { Component } from '@angular/core'
import { injectLiveQuery } from '@tanstack/angular-db'

@Component({
  selector: 'user-list',
  template: `
    @for (user of activeUsers.data(); track user.id) {
      <li>{{ user.name }}</li>
    }
  `
})
export class UserListComponent {
  activeUsers = injectLiveQuery((q) =>
    q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
  )
}
```

> **Note:** React hooks (`useLiveQuery`, `useLiveInfiniteQuery`, `useLiveSuspenseQuery`) accept an optional dependency array parameter to re-execute queries when values change, similar to React's `useEffect`. See the [React Adapter documentation](../framework/react/overview#dependency-arrays) for details on when and how to use dependency arrays.

For more details on framework integration, see the [React](../framework/react/overview), [Vue](../framework/vue/overview), and [Angular](../framework/angular/overview) adapter documentation.

### Using with React Suspense

For React applications, you can use the `useLiveSuspenseQuery` hook to integrate with React Suspense boundaries. This hook suspends rendering while data loads initially, then streams updates without re-suspending.

```tsx
import { useLiveSuspenseQuery } from '@tanstack/react-db'
import { Suspense } from 'react'

function UserList() {
  // This will suspend until data is ready
  const { data } = useLiveSuspenseQuery((q) =>
    q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
  )

  // data is always defined - no need for optional chaining
  return (
    <ul>
      {data.map(user => (
        <li key={user.id}>{user.name}</li>
      ))}
    </ul>
  )
}

function App() {
  return (
    <Suspense fallback={<div>Loading users...</div>}>
      <UserList />
    </Suspense>
  )
}
```

#### Type Safety

The key difference from `useLiveQuery` is that `data` is always defined (never `undefined`). The hook suspends during initial load, so by the time your component renders, data is guaranteed to be available:

```tsx
function UserStats() {
  const { data } = useLiveSuspenseQuery((q) =>
    q.from({ user: usersCollection })
  )

  // TypeScript knows data is Array<User>, not Array<User> | undefined
  return <div>Total users: {data.length}</div>
}
```

#### Error Handling

Combine with Error Boundaries to handle loading errors:

```tsx
import { ErrorBoundary } from 'react-error-boundary'

function App() {
  return (
    <ErrorBoundary fallback={<div>Failed to load users</div>}>
      <Suspense fallback={<div>Loading users...</div>}>
        <UserList />
      </Suspense>
    </ErrorBoundary>
  )
}
```

#### Reactive Updates

After the initial load, data updates stream in without re-suspending:

```tsx
function UserList() {
  const { data } = useLiveSuspenseQuery((q) =>
    q.from({ user: usersCollection })
  )

  // Suspends once during initial load
  // After that, data updates automatically when users change
  // UI never re-suspends for live updates
  return (
    <ul>
      {data.map(user => (
        <li key={user.id}>{user.name}</li>
      ))}
    </ul>
  )
}
```

#### Re-suspending on Dependency Changes

When dependencies change, the hook re-suspends to load new data:

```tsx
function FilteredUsers({ minAge }: { minAge: number }) {
  const { data } = useLiveSuspenseQuery(
    (q) =>
      q
        .from({ user: usersCollection })
        .where(({ user }) => gt(user.age, minAge)),
    [minAge] // Re-suspend when minAge changes
  )

  return (
    <ul>
      {data.map(user => (
        <li key={user.id}>{user.name} - {user.age}</li>
      ))}
    </ul>
  )
}
```

#### When to Use Which Hook

- **Use `useLiveSuspenseQuery`** when:
  - You want to use React Suspense for loading states
  - You prefer handling loading/error states with `<Suspense>` and `<ErrorBoundary>` components
  - You want guaranteed non-undefined data types
  - The query always needs to run (not conditional)

- **Use `useLiveQuery`** when:
  - You need conditional/disabled queries
  - You prefer handling loading/error states within your component
  - You want to show loading states inline without Suspense
  - You need access to `status` and `isLoading` flags
  - **You're using a router with loaders** (React Router, TanStack Router, etc.) - preload in the loader and use `useLiveQuery` in the component

```tsx
// useLiveQuery - handle states in component
function UserList() {
  const { data, status, isLoading } = useLiveQuery((q) =>
    q.from({ user: usersCollection })
  )

  if (isLoading) return <div>Loading...</div>
  if (status === 'error') return <div>Error loading users</div>

  return <ul>{data?.map(user => <li key={user.id}>{user.name}</li>)}</ul>
}

// useLiveSuspenseQuery - handle states with Suspense/ErrorBoundary
function UserList() {
  const { data } = useLiveSuspenseQuery((q) =>
    q.from({ user: usersCollection })
  )

  return <ul>{data.map(user => <li key={user.id}>{user.name}</li>)}</ul>
}

// useLiveQuery with router loader - recommended pattern
// In your route configuration:
const route = {
  path: '/users',
  loader: async () => {
    // Preload the collection in the loader
    await usersCollection.preload()
    return null
  },
  component: UserList,
}

// In your component:
function UserList() {
  // Collection is already loaded, so data is immediately available
  const { data } = useLiveQuery((q) =>
    q.from({ user: usersCollection })
  )

  return <ul>{data?.map(user => <li key={user.id}>{user.name}</li>)}</ul>
}
```

### Conditional Queries

In React, you can conditionally disable a query by returning `undefined` or `null` from the `useLiveQuery` callback. When disabled, the hook returns a special state indicating the query is not active.

```tsx
import { useLiveQuery } from '@tanstack/react-db'

function TodoList({ userId }: { userId?: string }) {
  const { data, isEnabled, status } = useLiveQuery((q) => {
    // Disable the query when userId is not available
    if (!userId) return undefined

    return q
      .from({ todos: todosCollection })
      .where(({ todos }) => eq(todos.userId, userId))
  }, [userId])

  if (!isEnabled) {
    return <div>Please select a user</div>
  }

  return (
    <ul>
      {data?.map(todo => (
        <li key={todo.id}>{todo.text}</li>
      ))}
    </ul>
  )
}
```

When the query is disabled (callback returns `undefined` or `null`):
- `status` is `'disabled'`
- `data`, `state`, and `collection` are `undefined`
- `isEnabled` is `false`
- `isLoading`, `isReady`, `isIdle`, and `isError` are all `false`

This pattern is useful for "wait until inputs exist" flows without needing to conditionally render the hook itself or manage an external enabled flag.

### Alternative Callback Return Types

The `useLiveQuery` callback can return different types depending on your use case:

#### Returning a Query Builder (Standard)

The most common pattern is to return a query builder:

```tsx
const { data } = useLiveQuery((q) =>
  q.from({ todos: todosCollection })
   .where(({ todos }) => eq(todos.completed, false))
)
```

#### Returning a Pre-created Collection

You can return an existing collection directly:

```tsx
const activeUsersCollection = createLiveQueryCollection((q) =>
  q.from({ users: usersCollection })
   .where(({ users }) => eq(users.active, true))
)

function UserList({ usePrebuilt }: { usePrebuilt: boolean }) {
  const { data } = useLiveQuery((q) => {
    // Toggle between pre-created collection and ad-hoc query
    if (usePrebuilt) return activeUsersCollection

    return q.from({ users: usersCollection })
  }, [usePrebuilt])

  return <ul>{data?.map(user => <li key={user.id}>{user.name}</li>)}</ul>
}
```

#### Returning a LiveQueryCollectionConfig

You can return a configuration object to specify additional options like a custom ID:

```tsx
const { data } = useLiveQuery((q) => {
  return {
    query: q.from({ items: itemsCollection })
             .select(({ items }) => ({ id: items.id })),
    id: 'items-view', // Custom ID for debugging
    gcTime: 10000 // Custom garbage collection time
  }
})
```

This is particularly useful when you need to:
- Attach a stable ID for debugging or logging
- Configure collection-specific options like `gcTime` or `getKey`
- Conditionally switch between different collection configurations

## From Clause

The foundation of every query is the `from` method, which specifies the source collection or subquery. You can alias the source using object syntax.

### Method Signature

```ts
from({
  [alias]: Collection | Query,
}): Query
```

**Parameters:**
- `[alias]` - A Collection or Query instance. Note that only a single aliased collection or subquery is allowed in the `from` clause.

### Basic Usage

Start with a basic query that selects all records from a collection:

```ts
const allUsers = createCollection(liveQueryCollectionOptions({
  query: (q) => q.from({ user: usersCollection })
}))
```

The result contains all users with their full schema. You can iterate over the results or access them by key:

```ts
// Get all users as an array
const users = allUsers.toArray

// Get a specific user by ID
const user = allUsers.get(1)

// Check if a user exists
const hasUser = allUsers.has(1)
```

Use aliases to make your queries more readable, especially when working with multiple collections:

```ts
const users = createCollection(liveQueryCollectionOptions({
  query: (q) => q.from({ u: usersCollection })
}))

// Access fields using the alias
const userNames = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ u: usersCollection })
      .select(({ u }) => ({
        name: u.name,
        email: u.email,
      }))
}))
```

## Where Clauses

Use `where` clauses to filter your data based on conditions. You can chain multiple `where` calls - they are combined with `and` logic.

The `where` method takes a callback function that receives an object containing your table aliases and returns a boolean expression. You build these expressions using comparison functions like `eq()`, `gt()`, and logical operators like `and()` and `or()`. This declarative approach allows the query system to optimize your filters efficiently. These are described in more detail in the [Expression Functions Reference](#expression-functions-reference) section. This is very similar to how you construct queries using Kysely or Drizzle.

It's important to note that the `where` method is not a function that is executed on each row or the results, its a way to describe the query that will be executed. This declarative approach works well for almost all use cases, but if you need to use a more complex condition, there is the functional variant as `fn.where` which is described in the [Functional Variants](#functional-variants) section.

### Method Signature

```ts
where(
  condition: (row: TRow) => Expression<boolean>
): Query
```

**Parameters:**
- `condition` - A callback function that receives the row object with table aliases and returns a boolean expression

### Basic Filtering

Filter users by a simple condition:

```ts
import { eq } from '@tanstack/db'

const activeUsers = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
}))
```

### Multiple Conditions

Chain multiple `where` calls for AND logic:

```ts
import { eq, gt } from '@tanstack/db'

const adultActiveUsers = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
      .where(({ user }) => gt(user.age, 18))
}))
```

### Complex Conditions

Use logical operators to build complex conditions:

```ts
import { eq, gt, or, and } from '@tanstack/db'

const specialUsers = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .where(({ user }) => 
      and(
        eq(user.active, true),
        or(
          gt(user.age, 25),
          eq(user.role, 'admin')
        )
      )
    )
)
```

### Available Operators

The query system provides several comparison operators:

```ts
import { eq, gt, gte, lt, lte, like, ilike, inArray, and, or, not } from '@tanstack/db'

// Equality
eq(user.id, 1)

// Comparisons
gt(user.age, 18)    // greater than
gte(user.age, 18)   // greater than or equal
lt(user.age, 65)    // less than
lte(user.age, 65)   // less than or equal

// String matching
like(user.name, 'John%')    // case-sensitive pattern matching
ilike(user.name, 'john%')   // case-insensitive pattern matching

// Array membership
inArray(user.id, [1, 2, 3])

// Logical operators
and(condition1, condition2)
or(condition1, condition2)
not(condition)
```

For a complete reference of all available functions, see the [Expression Functions Reference](#expression-functions-reference) section.

## Select

Use `select` to specify which fields to include in your results and transform your data. Without `select`, you get the full schema.

Similar to the `where` clause, the `select` method takes a callback function that receives an object containing your table aliases and returns an object with the fields you want to include in your results. These can be combined with functions from the [Expression Functions Reference](#expression-functions-reference) section to create computed fields. You can also use the spread operator to include all fields from a table.

### Method Signature

```ts
select(
  projection: (row: TRow) => Record<string, Expression>
): Query
```

**Parameters:**
- `projection` - A callback function that receives the row object with table aliases and returns the selected fields object

### Basic Selects

Select specific fields from your data:

````ts
const userNames = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .select(({ user }) => ({
      id: user.id,
      name: user.name,
      email: user.email,
    }))
)

/*
Result type: { id: number, name: string, email: string }

```ts
for (const row of userNames) {
  console.log(row.name)
}
```
*/
````

### Field Renaming

Rename fields in your results:

```ts
const userProfiles = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .select(({ user }) => ({
      userId: user.id,
      fullName: user.name,
      contactEmail: user.email,
    }))
)
```

### Computed Fields

Create computed fields using expressions:

```ts
import { gt, length } from '@tanstack/db'

const userStats = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .select(({ user }) => ({
      id: user.id,
      name: user.name,
      isAdult: gt(user.age, 18),
      nameLength: length(user.name),
    }))
)
```

### Using Functions and Including All Fields

Transform your data using built-in functions:

````ts
import { concat, upper, gt } from '@tanstack/db'

const formattedUsers = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .select(({ user }) => ({
        ...user, // Include all user fields
        displayName: upper(concat(user.firstName, ' ', user.lastName)),
        isAdult: gt(user.age, 18),
      }))
}))

/*
Result type:
{
  id: number,
  name: string,
  email: string,
  displayName: string,
  isAdult: boolean,
}
*/
````

For a complete list of available functions, see the [Expression Functions Reference](#expression-functions-reference) section.

## Joins

Use `join` to combine data from multiple collections. Joins default to `left` join type and only support equality conditions.

Joins in TanStack DB are a way to combine data from multiple collections, and are conceptually very similar to SQL joins. When two collections are joined, the result is a new collection that contains the combined data as single rows. The new collection is a live query collection, and will automatically update when the underlying data changes.

A `join` without a `select` will return row objects that are namespaced with the aliases of the joined collections.

The result type of a join will take into account the join type, with the optionality of the joined fields being determined by the join type.

> [!NOTE]
> We are working on an `include` system that will enable joins that project to a hierarchical object. For example an `issue` row could have a `comments` property that is an array of `comment` rows.
> See [this issue](https://github.com/TanStack/db/issues/288) for more details.

### Method Signature

```ts
join(
  { [alias]: Collection | Query },
  condition: (row: TRow) => Expression<boolean>, // Must be an `eq` condition
  joinType?: 'left' | 'right' | 'inner' | 'full'
): Query
```

**Parameters:**
- `aliases` - An object where keys are alias names and values are collections or subqueries to join
- `condition` - A callback function that receives the combined row object and returns an equality condition
- `joinType` - Optional join type: `'left'` (default), `'right'`, `'inner'`, or `'full'`

### Basic Joins

Join users with their posts:

````ts
const userPosts = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .join({ post: postsCollection }, ({ user, post }) => 
      eq(user.id, post.userId)
    )
)

/*
Result type: 
{ 
  user: User,
  post?: Post, // post is optional because it is a left join
}

```ts
for (const row of userPosts) {
  console.log(row.user.name, row.post?.title)
}
```
*/
````

### Join Types

Specify the join type as the third parameter:

```ts
const activeUserPosts = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .join(
      { post: postsCollection }, 
      ({ user, post }) => eq(user.id, post.userId),
      'inner', // `inner`, `left`, `right` or `full`
    )
)
```

Or using the aliases `leftJoin`, `rightJoin`, `innerJoin` and `fullJoin` methods:

### Left Join
```ts
// Left join - all users, even without posts
const allUsers = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .leftJoin(
      { post: postsCollection }, 
      ({ user, post }) => eq(user.id, post.userId),
    )
)

/*
Result type:
{
  user: User,
  post?: Post, // post is optional because it is a left join
}
*/
```

### Right Join

```ts
// Right join - all posts, even without users
const allPosts = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .rightJoin(
      { post: postsCollection }, 
      ({ user, post }) => eq(user.id, post.userId),
    )
)

/*
Result type:
{
  user?: User, // user is optional because it is a right join
  post: Post,
}
*/
```

### Inner Join

```ts
// Inner join - only matching records
const activeUserPosts = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .innerJoin(
      { post: postsCollection }, 
      ({ user, post }) => eq(user.id, post.userId),
    )
)

/*
Result type:
{
  user: User,
  post: Post,
}
*/
```

### Full Join

```ts
// Full join - all users and all posts
const allUsersAndPosts = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .fullJoin(
      { post: postsCollection }, 
      ({ user, post }) => eq(user.id, post.userId),
    )
)

/*
Result type:
{
  user?: User, // user is optional because it is a full join
  post?: Post, // post is optional because it is a full join
}
*/
```

### Multiple Joins

Chain multiple joins in a single query:

```ts
const userPostComments = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .join({ post: postsCollection }, ({ user, post }) => 
      eq(user.id, post.userId)
    )
    .join({ comment: commentsCollection }, ({ post, comment }) => 
      eq(post.id, comment.postId)
    )
    .select(({ user, post, comment }) => ({
      userName: user.name,
      postTitle: post.title,
      commentText: comment.text,
    }))
)
```

## Subqueries

Subqueries allow you to use the result of one query as input to another, they are embedded within the query itself and are compile to a single query pipeline. They are very similar to SQL subqueries that are executed as part of a single operation.

Note that subqueries are not the same as using a live query result in a `from` or `join` clause in a new query. When you do that the intermediate result is fully computed and accessible to you, subqueries are internal to their parent query and not materialised to a collection themselves and so are more efficient.

See the [Caching Intermediate Results](#caching-intermediate-results) section for more details on using live query results in a `from` or `join` clause in a new query.

### Subqueries in `from` Clauses

Use a subquery as the main source:

```ts
const activeUserPosts = createCollection(liveQueryCollectionOptions({
  query: (q) => {
    // Build the subquery first
    const activeUsers = q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
    
    // Use the subquery in the main query
    return q
      .from({ activeUser: activeUsers })
      .join({ post: postsCollection }, ({ activeUser, post }) => 
        eq(activeUser.id, post.userId)
      )
  }
}))
```

### Subqueries in `join` Clauses

Join with a subquery result:

```ts
const userRecentPosts = createCollection(liveQueryCollectionOptions({
  query: (q) => {
    // Build the subquery first
    const recentPosts = q
      .from({ post: postsCollection })
      .where(({ post }) => gt(post.createdAt, '2024-01-01'))
      .orderBy(({ post }) => post.createdAt, 'desc')
      .limit(1)
    
    // Use the subquery in the main query
    return q
      .from({ user: usersCollection })
      .join({ recentPost: recentPosts }, ({ user, recentPost }) => 
        eq(user.id, recentPost.userId)
      )
  }
}))
```

### Subquery deduplication  

When the same subquery is used multiple times within a query, it's automatically deduplicated and executed only once:

```ts
const complexQuery = createCollection(liveQueryCollectionOptions({
  query: (q) => {
    // Build the subquery once
    const activeUsers = q
      .from({ user: usersCollection })
      .where(({ user }) => eq(user.active, true))
    
    // Use the same subquery multiple times
    return q
      .from({ activeUser: activeUsers })
      .join({ post: postsCollection }, ({ activeUser, post }) => 
        eq(activeUser.id, post.userId)
      )
      .join({ comment: commentsCollection }, ({ activeUser, comment }) => 
        eq(activeUser.id, comment.userId)
      )
  }
}))
```

In this example, the `activeUsers` subquery is used twice but executed only once, improving performance.

### Complex Nested Subqueries

Build complex queries with multiple levels of nesting:

```ts
import { count } from '@tanstack/db'

const topUsers = createCollection(liveQueryCollectionOptions({
  query: (q) => {
    // Build the post count subquery
    const postCounts = q
      .from({ post: postsCollection })
      .groupBy(({ post }) => post.userId)
      .select(({ post }) => ({
        userId: post.userId,
        count: count(post.id),
      }))
    
    // Build the user stats subquery
    const userStats = q
      .from({ user: usersCollection })
      .join({ postCount: postCounts }, ({ user, postCount }) => 
        eq(user.id, postCount.userId)
      )
      .select(({ user, postCount }) => ({
        id: user.id,
        name: user.name,
        postCount: postCount.count,
      }))
      .orderBy(({ userStats }) => userStats.postCount, 'desc')
      .limit(10)
    
    // Use the user stats subquery in the main query
    return q.from({ userStats })
  }
}))
```

## groupBy and Aggregations

Use `groupBy` to group your data and apply aggregate functions. When you use aggregates in `select` without `groupBy`, the entire result set is treated as a single group.

### Method Signature

```ts
groupBy(
  grouper: (row: TRow) => Expression | Expression[]
): Query
```

**Parameters:**
- `grouper` - A callback function that receives the row object and returns the grouping key(s). Can return a single value or an array for multi-column grouping

### Basic Grouping

Group users by their department and count them:

```ts
import { count, avg } from '@tanstack/db'

const departmentStats = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .groupBy(({ user }) => user.departmentId)
      .select(({ user }) => ({
        departmentId: user.departmentId,
        userCount: count(user.id),
        avgAge: avg(user.age),
      }))
}))
```

> [!NOTE]
> In `groupBy` queries, the properties in your `select` clause must either be:
> - An aggregate function (like `count`, `sum`, `avg`)
> - A property that was used in the `groupBy` clause
> 
> You cannot select properties that are neither aggregated nor grouped.

### Multiple Column Grouping

Group by multiple columns by returning an array from the callback:

```ts
const userStats = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .groupBy(({ user }) => [user.departmentId, user.role])
      .select(({ user }) => ({
        departmentId: user.departmentId,
        role: user.role,
        count: count(user.id),
        avgSalary: avg(user.salary),
      }))
}))
```

### Aggregate Functions

Use various aggregate functions to summarize your data:

```ts
import { count, sum, avg, min, max } from '@tanstack/db'

const orderStats = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ order: ordersCollection })
      .groupBy(({ order }) => order.customerId)
      .select(({ order }) => ({
        customerId: order.customerId,
        totalOrders: count(order.id),
        totalAmount: sum(order.amount),
        avgOrderValue: avg(order.amount),
        minOrder: min(order.amount),
        maxOrder: max(order.amount),
      }))
}))
```

See the [Aggregate Functions](#aggregate-functions) section for a complete list of available aggregate functions.

### Having Clauses

Filter aggregated results using `having` - this is similar to the `where` clause, but is applied after the aggregation has been performed.

#### Method Signature

```ts
having(
  condition: (row: TRow) => Expression<boolean>
): Query
```

**Parameters:**
- `condition` - A callback function that receives the aggregated row object and returns a boolean expression

```ts
const highValueCustomers = createLiveQueryCollection((q) =>
  q
    .from({ order: ordersCollection })
    .groupBy(({ order }) => order.customerId)
    .select(({ order }) => ({
      customerId: order.customerId,
      totalSpent: sum(order.amount),
      orderCount: count(order.id),
    }))
    .having(({ order }) => gt(sum(order.amount), 1000))
)
```

### Implicit Single-Group Aggregation

When you use aggregates without `groupBy`, the entire result set is grouped:

```ts
const overallStats = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .select(({ user }) => ({
      totalUsers: count(user.id),
      avgAge: avg(user.age),
      maxSalary: max(user.salary),
    }))
)
```

This is equivalent to grouping the entire collection into a single group.

### Accessing Grouped Data

Grouped results can be accessed by the group key:

```ts
const deptStats = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .groupBy(({ user }) => user.departmentId)
      .select(({ user }) => ({
        departmentId: user.departmentId,
        count: count(user.id),
      }))
}))

// Access by department ID
const engineeringStats = deptStats.get(1)
```

> **Note**: Grouped results are keyed differently based on the grouping:
> - **Single column grouping**: Keyed by the actual value (e.g., `deptStats.get(1)`)
> - **Multiple column grouping**: Keyed by a JSON string of the grouped values (e.g., `userStats.get('[1,"admin"]')`)

## findOne

Use `findOne` to return a single result instead of an array. This is useful when you expect to find at most one matching record, such as when querying by a unique identifier.

The `findOne` method changes the return type from an array to a single object or `undefined`. When no matching record is found, the result is `undefined`.

### Method Signature

```ts
findOne(): Query
```

### Basic Usage

Find a specific user by ID:

```ts
const user = createLiveQueryCollection((q) =>
  q
    .from({ users: usersCollection })
    .where(({ users }) => eq(users.id, 1))
    .findOne()
)

// Result type: User | undefined
// If user with id=1 exists: { id: 1, name: 'John', ... }
// If not found: undefined
```

### With React Hooks

Use `findOne` with `useLiveQuery` to get a single record:

```tsx
import { useLiveQuery } from '@tanstack/react-db'
import { eq } from '@tanstack/db'

function UserProfile({ userId }: { userId: string }) {
  const { data: user, isLoading } = useLiveQuery((q) =>
    q
      .from({ users: usersCollection })
      .where(({ users }) => eq(users.id, userId))
      .findOne()
  , [userId])

  if (isLoading) return <div>Loading...</div>
  if (!user) return <div>User not found</div>

  return <div>{user.name}</div>
}
```

### With Select

Combine `findOne` with `select` to project specific fields:

```ts
const userEmail = createLiveQueryCollection((q) =>
  q
    .from({ users: usersCollection })
    .where(({ users }) => eq(users.id, 1))
    .select(({ users }) => ({
      id: users.id,
      email: users.email,
    }))
    .findOne()
)

// Result type: { id: number, email: string } | undefined
```

### Return Type Behavior

The return type changes based on whether `findOne` is used:

```ts
// Without findOne - returns array
const users = createLiveQueryCollection((q) =>
  q.from({ users: usersCollection })
)
// Type: Array<User>

// With findOne - returns single object or undefined
const user = createLiveQueryCollection((q) =>
  q.from({ users: usersCollection }).findOne()
)
// Type: User | undefined
```

### Best Practices

**Use when:**
- Querying by unique identifiers (ID, email, etc.)
- You expect at most one result
- You want type-safe single-record access without array indexing

**Avoid when:**
- You might have multiple matching records (use regular queries instead)
- You need to iterate over results

## Distinct

Use `distinct` to remove duplicate rows from your query results based on the selected columns. The `distinct` operator ensures that each unique combination of selected values appears only once in the result set.

> [!IMPORTANT]
> The `distinct` operator requires a `select` clause. You cannot use `distinct` without specifying which columns to select.

### Method Signature

```ts
distinct(): Query
```

### Basic Usage

Get unique values from a single column:

```ts
const uniqueCountries = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .select(({ user }) => ({ country: user.country }))
    .distinct()
)

// Result contains only unique countries
// If you have users from USA, Canada, and UK, the result will have 3 items
```

### Multiple Column Distinct

Get unique combinations of multiple columns:

```ts
const uniqueRoleSalaryPairs = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .select(({ user }) => ({
      role: user.role,
      salary: user.salary,
    }))
    .distinct()
)

// Result contains only unique role-salary combinations
// e.g., Developer-75000, Developer-80000, Manager-90000
```

### Edge Cases

#### Null Values

Null values are treated as distinct values:

```ts
const uniqueValues = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .select(({ user }) => ({ department: user.department }))
    .distinct()
)

// If some users have null departments, null will appear as a distinct value
// Result might be: ['Engineering', 'Marketing', null]
```

## Order By, Limit, and Offset

Use `orderBy`, `limit`, and `offset` to control the order and pagination of your results. Ordering is performed incrementally for optimal performance.

### Method Signatures

```ts
orderBy(
  selector: (row: TRow) => Expression,
  direction?: 'asc' | 'desc'
): Query

limit(count: number): Query

offset(count: number): Query
```

**Parameters:**
- `selector` - A callback function that receives the row object and returns the value to sort by
- `direction` - Sort direction: `'asc'` (default) or `'desc'`
- `count` - Number of rows to limit or skip

### Basic Ordering

Sort results by a single column:

```ts
const sortedUsers = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .orderBy(({ user }) => user.name)
    .select(({ user }) => ({
      id: user.id,
      name: user.name,
    }))
)
```

### Multiple Column Ordering

Order by multiple columns:

```ts
const sortedUsers = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .orderBy(({ user }) => user.departmentId, 'asc')
    .orderBy(({ user }) => user.name, 'asc')
    .select(({ user }) => ({
      id: user.id,
      name: user.name,
      departmentId: user.departmentId,
    }))
)
```

### Descending Order

Use `desc` for descending order:

```ts
const recentPosts = createLiveQueryCollection((q) =>
  q
    .from({ post: postsCollection })
    .orderBy(({ post }) => post.createdAt, 'desc')
    .select(({ post }) => ({
      id: post.id,
      title: post.title,
      createdAt: post.createdAt,
    }))
)
```

### Pagination with `limit` and `offset`

Skip results using `offset`:

```ts
const page2Users = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .orderBy(({ user }) => user.name, 'asc')
    .limit(20)
    .offset(20) // Skip first 20 results
    .select(({ user }) => ({
      id: user.id,
      name: user.name,
    }))
)
```

## Composable Queries

Build complex queries by composing smaller, reusable parts. This approach makes your queries more maintainable and allows for better performance through caching.

### Conditional Query Building

Build queries based on runtime conditions:

```ts
import { Query, eq } from '@tanstack/db'

function buildUserQuery(options: { activeOnly?: boolean; limit?: number }) {
  let query = new Query().from({ user: usersCollection })
  
  if (options.activeOnly) {
    query = query.where(({ user }) => eq(user.active, true))
  }
  
  if (options.limit) {
    query = query.limit(options.limit)
  }
  
  return query.select(({ user }) => ({
    id: user.id,
    name: user.name,
  }))
}

const activeUsers = createLiveQueryCollection(buildUserQuery({ activeOnly: true, limit: 10 }))
```

### Caching Intermediate Results

The result of a live query collection is a collection itself, and will automatically update when the underlying data changes. This means that you can use the result of a live query collection as a source in another live query collection. This pattern is useful for building complex queries where you want to cache intermediate results to make further queries faster.

```ts
// Base query for active users
const activeUsers = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .where(({ user }) => eq(user.active, true))
)

// Query that depends on active users
const activeUserPosts = createLiveQueryCollection((q) =>
  q
    .from({ user: activeUsers })
    .join({ post: postsCollection }, ({ user, post }) => 
      eq(user.id, post.userId)
    )
    .select(({ user, post }) => ({
      userName: user.name,
      postTitle: post.title,
    }))
)
```

### Reusable Query Definitions

You can use the `Query` class to create reusable query definitions. This is useful for building complex queries where you want to reuse the same query builder instance multiple times throughout your application.

```ts
import { Query, eq } from '@tanstack/db'

// Create a reusable query builder
const userQuery = new Query()
  .from({ user: usersCollection })
  .where(({ user }) => eq(user.active, true))

// Use it in different contexts
const activeUsers = createLiveQueryCollection({
  query: userQuery.select(({ user }) => ({
    id: user.id,
    name: user.name,
  }))
})

// Or as a subquery
const userPosts = createLiveQueryCollection((q) =>
  q
    .from({ activeUser: userQuery })
    .join({ post: postsCollection }, ({ activeUser, post }) => 
      eq(activeUser.id, post.userId)
    )
)
```

### Reusable Callback Functions

Creating reusable query logic is a common pattern that improves code organization and maintainability. The recommended approach is to use callback functions with the `Ref<T>` type rather than trying to type `QueryBuilder` instances directly.

#### The Recommended Pattern

Use `Ref<MyType>` to create reusable filter and transform functions:

```ts
import type { Ref } from '@tanstack/db'
import { eq, gt, and } from '@tanstack/db'

// Create reusable filter callbacks
const isActiveUser = ({ user }: { user: Ref<User> }) =>
  eq(user.active, true)

const isAdultUser = ({ user }: { user: Ref<User> }) =>
  gt(user.age, 18)

const isActiveAdult = ({ user }: { user: Ref<User> }) =>
  and(isActiveUser({ user }), isAdultUser({ user }))

// Use them in queries - they work seamlessly with .where()
const activeAdults = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .where(isActiveUser)
      .where(isAdultUser)
      .select(({ user }) => ({
        id: user.id,
        name: user.name,
        age: user.age,
      }))
}))
```

The callback signature `({ user }: { user: Ref<User> }) => Expression` matches exactly what `.where()` expects, making it type-safe and composable.

#### Chaining Multiple Filters

You can chain multiple reusable filters:

```tsx
import { useLiveQuery } from '@tanstack/react-db'

const { data } = useLiveQuery((q) => {
  return q
    .from({ item: itemsCollection })
    .where(({ item }) => eq(item.id, 1))
    .where(activeItemFilter)      // Reusable filter 1
    .where(verifiedItemFilter)     // Reusable filter 2
    .select(({ item }) => ({ ...item }))
}, [])
```

#### Using with Different Aliases

The pattern works with any table alias:

```ts
const activeFilter = ({ item }: { item: Ref<Item> }) =>
  eq(item.active, true)

// Works with any alias name
const query1 = new Query()
  .from({ item: itemsCollection })
  .where(activeFilter)

const query2 = new Query()
  .from({ i: itemsCollection })
  .where(({ i }) => activeFilter({ item: i }))  // Map the alias
```

#### Callbacks with Multiple Tables

For queries with joins, create callbacks that accept multiple refs:

```ts
const isHighValueCustomer = ({ user, order }: {
  user: Ref<User>
  order: Ref<Order>
}) => and(
  eq(user.active, true),
  gt(order.amount, 1000)
)

// Use directly in where clause
const highValueCustomers = createCollection(liveQueryCollectionOptions({
  query: (q) =>
    q
      .from({ user: usersCollection })
      .join({ order: ordersCollection }, ({ user, order }) =>
        eq(user.id, order.userId)
      )
      .where(isHighValueCustomer)
      .select(({ user, order }) => ({
        userName: user.name,
        orderAmount: order.amount,
      }))
}))
```

#### Why Not Type QueryBuilder?

You might be tempted to create functions that accept and return `QueryBuilder`:

```ts
// ❌ Not recommended - overly complex typing
const applyFilters = <T extends QueryBuilder<unknown>>(query: T): T => {
  return query.where(({ item }) => eq(item.active, true))
}
```

This approach has several issues:

1. **Complex Types**: `QueryBuilder<T>` generic represents the entire query context including base schema, current schema, joins, result types, etc.
2. **Type Inference**: The type changes with every method call, making it impractical to type manually
3. **Limited Flexibility**: Hard to compose multiple filters or use with different table aliases

Instead, use callback functions that work with the `.where()`, `.select()`, and other query methods directly.

#### Reusable Select Transformations

You can also create reusable select projections:

```ts
const basicUserInfo = ({ user }: { user: Ref<User> }) => ({
  id: user.id,
  name: user.name,
  email: user.email,
})

const userWithStats = ({ user }: { user: Ref<User> }) => ({
  ...basicUserInfo({ user }),
  isAdult: gt(user.age, 18),
  isActive: eq(user.active, true),
})

const users = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .select(userWithStats)
)
```

This approach makes your query logic more modular, testable, and reusable across your application.

## Expression Functions Reference

The query system provides a comprehensive set of functions for filtering, transforming, and aggregating data.

### Comparison Operators

#### `eq(left, right)`
Equality comparison:
```ts
eq(user.id, 1)
eq(user.name, 'John')
```

#### `gt(left, right)`, `gte(left, right)`, `lt(left, right)`, `lte(left, right)`
Numeric, string and date comparisons:
```ts
gt(user.age, 18)
gte(user.salary, 50000)
lt(user.createdAt, new Date('2024-01-01'))
lte(user.rating, 5)
```

#### `inArray(value, array)`
Check if a value is in an array:
```ts
inArray(user.id, [1, 2, 3])
inArray(user.role, ['admin', 'moderator'])
```

#### `like(value, pattern)`, `ilike(value, pattern)`
String pattern matching:
```ts
like(user.name, 'John%')    // Case-sensitive
ilike(user.email, '%@gmail.com')  // Case-insensitive
```

#### `isUndefined(value)`, `isNull(value)`
Check for missing vs null values:
```ts
// Check if a property is missing/undefined
isUndefined(user.profile)

// Check if a value is explicitly null
isNull(user.profile)
```

These functions are particularly important when working with joins and optional properties, as they distinguish between:
- `undefined`: The property is absent or not present
- `null`: The property exists but is explicitly set to null

**Example with joins:**
```ts
// Find users without a matching profile (left join resulted in undefined)
const usersWithoutProfiles = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .leftJoin(
      { profile: profilesCollection },
      ({ user, profile }) => eq(user.id, profile.userId)
    )
    .where(({ profile }) => isUndefined(profile))
)

// Find users with explicitly null bio field
const usersWithNullBio = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .where(({ user }) => isNull(user.bio))
)
```

### Logical Operators

#### `and(...conditions)`
Combine conditions with AND logic:
```ts
and(
  eq(user.active, true),
  gt(user.age, 18),
  eq(user.role, 'user')
)
```

#### `or(...conditions)`
Combine conditions with OR logic:
```ts
or(
  eq(user.role, 'admin'),
  eq(user.role, 'moderator')
)
```

#### `not(condition)`
Negate a condition:
```ts
not(eq(user.active, false))
```

### String Functions

#### `upper(value)`, `lower(value)`
Convert case:
```ts
upper(user.name)  // 'JOHN'
lower(user.email) // 'john@example.com'
```

#### `length(value)`
Get string or array length:
```ts
length(user.name)     // String length
length(user.tags)     // Array length
```

#### `concat(...values)`
Concatenate strings:
```ts
concat(user.firstName, ' ', user.lastName)
concat('User: ', user.name, ' (', user.id, ')')
```

### Mathematical Functions

#### `add(left, right)`
Add two numbers:
```ts
add(user.salary, user.bonus)
```

#### `coalesce(...values)`
Return the first non-null value:
```ts
coalesce(user.displayName, user.name, 'Unknown')
```

### Aggregate Functions

#### `count(value)`
Count non-null values:
```ts
count(user.id)        // Count all users
count(user.postId)    // Count users with posts
```

#### `sum(value)`
Sum numeric values:
```ts
sum(order.amount)
sum(user.salary)
```

#### `avg(value)`
Calculate average:
```ts
avg(user.salary)
avg(order.amount)
```

#### `min(value)`, `max(value)`
Find minimum and maximum values:
```ts
min(user.salary)
max(order.amount)
```

### Function Composition

Functions can be composed and chained:

```ts
// Complex condition
and(
  eq(user.active, true),
  or(
    gt(user.age, 25),
    eq(user.role, 'admin')
  ),
  not(inArray(user.id, bannedUserIds))
)

// Complex transformation
concat(
  upper(user.firstName),
  ' ',
  upper(user.lastName),
  ' (',
  user.id,
  ')'
)

// Complex aggregation
avg(add(user.salary, coalesce(user.bonus, 0)))
```

## Functional Variants

The functional variant API provides an alternative to the standard API, offering more flexibility for complex transformations. With functional variants, the callback functions contain actual code that gets executed to perform the operation, giving you the full power of JavaScript at your disposal.

> [!WARNING]
> The functional variant API cannot be optimized by the query optimizer or use collection indexes. It is intended for use in rare cases where the standard API is not sufficient.

### Functional Select

Use `fn.select()` for complex transformations with JavaScript logic:

```ts
const userProfiles = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .fn.select((row) => ({
      id: row.user.id,
      displayName: `${row.user.firstName} ${row.user.lastName}`,
      salaryTier: row.user.salary > 100000 ? 'senior' : 'junior',
      emailDomain: row.user.email.split('@')[1],
      isHighEarner: row.user.salary > 75000,
    }))
)
```

### Functional Where

Use `fn.where()` for complex filtering logic:

```ts
const specialUsers = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .fn.where((row) => {
      const user = row.user
      return user.active && 
             (user.age > 25 || user.role === 'admin') &&
             user.email.includes('@company.com')
    })
)
```

### Functional Having

Use `fn.having()` for complex aggregation filtering:

```ts
const highValueCustomers = createLiveQueryCollection((q) =>
  q
    .from({ order: ordersCollection })
    .groupBy(({ order }) => order.customerId)
    .select(({ order }) => ({
      customerId: order.customerId,
      totalSpent: sum(order.amount),
      orderCount: count(order.id),
    }))
    .fn.having((row) => {
      return row.totalSpent > 1000 && row.orderCount >= 3
    })
)
```

### Complex Transformations

Functional variants excel at complex data transformations:

```ts
const userProfiles = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .fn.select((row) => {
      const user = row.user
      const fullName = `${user.firstName} ${user.lastName}`.trim()
      const emailDomain = user.email.split('@')[1]
      const ageGroup = user.age < 25 ? 'young' : user.age < 50 ? 'adult' : 'senior'
      
      return {
        userId: user.id,
        displayName: fullName || user.name,
        contactInfo: {
          email: user.email,
          domain: emailDomain,
          isCompanyEmail: emailDomain === 'company.com'
        },
        demographics: {
          age: user.age,
          ageGroup: ageGroup,
          isAdult: user.age >= 18
        },
        status: user.active ? 'active' : 'inactive',
        profileStrength: fullName && user.email && user.age ? 'complete' : 'incomplete'
      }
    })
)
```

### Type Inference

Functional variants maintain full TypeScript support:

```ts
const processedUsers = createLiveQueryCollection((q) =>
  q
    .from({ user: usersCollection })
    .fn.select((row): ProcessedUser => ({
      id: row.user.id,
      name: row.user.name.toUpperCase(),
      age: row.user.age,
      ageGroup: row.user.age < 25 ? 'young' : row.user.age < 50 ? 'adult' : 'senior',
    }))
)
```

### When to Use Functional Variants

Use functional variants when you need:
- Complex JavaScript logic that can't be expressed with built-in functions
- Integration with external libraries or utilities
- Full JavaScript power for custom operations

The callbacks in functional variants are actual JavaScript functions that get executed, unlike the standard API which uses declarative expressions. This gives you complete control over the logic but comes with the trade-off of reduced optimization opportunities.

However, prefer the standard API when possible, as it provides better performance and optimization opportunities.


---


---
title: Mutations
id: mutations
---

# TanStack DB Mutations

TanStack DB provides a powerful mutation system that enables optimistic updates with automatic state management. This system is built around a pattern of **optimistic mutation → backend persistence → sync back → confirmed state**. This creates a highly responsive user experience while maintaining data consistency and being easy to reason about.

Local changes are applied immediately as optimistic state, then persisted to your backend, and finally the optimistic state is replaced by the confirmed server state once it syncs back.

```tsx
// Define a collection with a mutation handler
const todoCollection = createCollection({
  id: "todos",
  onUpdate: async ({ transaction }) => {
    const mutation = transaction.mutations[0]
    await api.todos.update(mutation.original.id, mutation.changes)
  },
})

// Apply an optimistic update
todoCollection.update(todo.id, (draft) => {
  draft.completed = true
})
```

This pattern extends the Redux/Flux unidirectional data flow beyond the client to include the server:

<figure>
  <a href="https://raw.githubusercontent.com/TanStack/db/main/docs/unidirectional-data-flow.lg.png" target="_blank">
    <img src="https://raw.githubusercontent.com/TanStack/db/main/docs/unidirectional-data-flow.png" />
  </a>
</figure>

With an instant inner loop of optimistic state, superseded in time by the slower outer loop of persisting to the server and syncing the updated server state back into the collection.

### Simplified Mutations vs Traditional Approaches

TanStack DB's mutation system eliminates much of the boilerplate required for optimistic updates in traditional approaches. Compare the difference:

**Before (TanStack Query with manual optimistic updates):**

```typescript
const addTodoMutation = useMutation({
  mutationFn: async (newTodo) => api.todos.create(newTodo),
  onMutate: async (newTodo) => {
    await queryClient.cancelQueries({ queryKey: ['todos'] })
    const previousTodos = queryClient.getQueryData(['todos'])
    queryClient.setQueryData(['todos'], (old) => [...(old || []), newTodo])
    return { previousTodos }
  },
  onError: (err, newTodo, context) => {
    queryClient.setQueryData(['todos'], context.previousTodos)
  },
  onSettled: () => {
    queryClient.invalidateQueries({ queryKey: ['todos'] })
  },
})
```

**After (TanStack DB):**

```typescript
const todoCollection = createCollection(
  queryCollectionOptions({
    queryKey: ['todos'],
    queryFn: async () => api.todos.getAll(),
    getKey: (item) => item.id,
    schema: todoSchema,
    onInsert: async ({ transaction }) => {
      await Promise.all(
        transaction.mutations.map((mutation) =>
          api.todos.create(mutation.modified)
        )
      )
    },
  })
)

// Simple mutation - no boilerplate!
todoCollection.insert({
  id: crypto.randomUUID(),
  text: '🔥 Make app faster',
  completed: false,
})
```

The benefits:
- ✅ Automatic optimistic updates
- ✅ Automatic rollback on error
- ✅ No manual cache manipulation
- ✅ Type-safe mutations

## Table of Contents

- [Mutation Approaches](#mutation-approaches)
- [Mutation Lifecycle](#mutation-lifecycle)
- [Collection Write Operations](#collection-write-operations)
- [Operation Handlers](#operation-handlers)
- [Creating Custom Actions](#creating-custom-actions)
- [Manual Transactions](#manual-transactions)
- [Paced Mutations](#paced-mutations)
- [Mutation Merging](#mutation-merging)
- [Controlling Optimistic Behavior](#controlling-optimistic-behavior)
- [Transaction States](#transaction-states)
- [Handling Temporary IDs](#handling-temporary-ids)

## Mutation Approaches

TanStack DB provides different approaches to mutations, each suited to different use cases:

### Collection-Level Mutations

Collection-level mutations (`insert`, `update`, `delete`) are designed for **direct state manipulation** of a single collection. These are the simplest way to make changes and work well for straightforward CRUD operations.

```tsx
// Direct state change
todoCollection.update(todoId, (draft) => {
  draft.completed = true
  draft.completedAt = new Date()
})
```

Use collection-level mutations when:
- You're making simple CRUD operations on a single collection
- The state changes are straightforward and match what the server will store

You can use `metadata` to annotate these operations and customize behavior in your handlers:

```tsx
// Annotate with metadata
todoCollection.update(
  todoId,
  { metadata: { intent: 'complete' } },
  (draft) => {
    draft.completed = true
  }
)

// Use metadata in handler
onUpdate: async ({ transaction }) => {
  const mutation = transaction.mutations[0]

  if (mutation.metadata?.intent === 'complete') {
    await Promise.all(
      transaction.mutations.map((mutation) =>
        api.todos.complete(mutation.original.id)
      )
    )
  } else {
    await Promise.all(
      transaction.mutations.map((mutation) =>
        api.todos.update(mutation.original.id, mutation.changes)
      )
    )
  }
}
```

### Intent-Based Mutations with Custom Actions

For more complex scenarios, use `createOptimisticAction` to create **intent-based mutations** that capture specific user actions.

```tsx
// Intent: "like this post"
const likePost = createOptimisticAction<string>({
  onMutate: (postId) => {
    // Optimistic guess at the change
    postCollection.update(postId, (draft) => {
      draft.likeCount += 1
      draft.likedByMe = true
    })
  },
  mutationFn: async (postId) => {
    // Send the intent to the server
    await api.posts.like(postId)
    // Server determines actual state changes
    await postCollection.utils.refetch()
  },
})

// Use it.
likePost(postId)
```

Use custom actions when:
- You need to mutate **multiple collections** in a single transaction
- The optimistic change is a **guess** at how the server will transform the data
- You want to send **user intent** to the backend rather than exact state changes
- The server performs complex logic, calculations, or side effects
- You want a clean, reusable mutation that captures a specific operation

Custom actions provide the cleanest way to capture specific types of mutations as named operations in your application. While you can achieve similar results using metadata with collection-level mutations, custom actions make the intent explicit and keep related logic together.

**When to use each:**

- **Collection-level mutations** (`collection.update`): Simple CRUD operations on a single collection
- **`createOptimisticAction`**: Intent-based operations, multi-collection mutations, immediately committed
- **Bypass the mutation system**: Use your existing mutation logic without rewriting

### Bypass the Mutation System

If you already have mutation logic in an existing system and don't want to rewrite it, you can **completely bypass** TanStack DB's mutation system and use your existing patterns.

With this approach, you write to the server like normal using your existing logic, then use your collection's mechanism for refetching or syncing data to await the server write. After the sync completes, the collection will have the updated server data and you can render the new state, hide loading indicators, show success messages, navigate to a new page, etc.

```tsx
// Call your backend directly with your existing logic
const handleUpdateTodo = async (todoId, changes) => {
  await api.todos.update(todoId, changes)

  // Wait for the server change to load into the collection
  await todoCollection.utils.refetch()

  // Now you know the new data is loaded and can render it or hide loaders
}

// With Electric
const handleUpdateTodo = async (todoId, changes) => {
  const { txid } = await api.todos.update(todoId, changes)

  // Wait for this specific transaction to sync into the collection
  await todoCollection.utils.awaitTxId(txid)

  // Now the server change is loaded and you can update UI accordingly
}
```

Use this approach when:
- You have existing mutation logic you don't want to rewrite
- You're comfortable with your current mutation patterns
- You want to use TanStack DB only for queries and state management

How to sync changes back:
- **QueryCollection**: Manually refetch with `collection.utils.refetch()` to reload data from the server
- **ElectricCollection**: Use `collection.utils.awaitTxId(txid)` to wait for a specific transaction to sync
- **Other sync systems**: Wait for your sync mechanism to update the collection

## Mutation Lifecycle

The mutation lifecycle follows a consistent pattern across all mutation types:

1. **Optimistic state applied**: The mutation is immediately applied to the local collection as optimistic state
2. **Handler invoked**: The appropriate handler — either `mutationFn` or a Collection handler (`onInsert`, `onUpdate`, or `onDelete`) — is called to persist the change
3. **Backend persistence**: Your handler persists the data to your backend
4. **Sync back**: The handler ensures server writes have synced back to the collection
5. **Optimistic state dropped**: Once synced, the optimistic state is replaced by the confirmed server state

```tsx
// Step 1: Optimistic state applied immediately
todoCollection.update(todo.id, (draft) => {
  draft.completed = true
})
// UI updates instantly with optimistic state

// Step 2-3: onUpdate handler persists to backend
// Step 4: Handler waits for sync back
// Step 5: Optimistic state replaced by server state
```

If the handler throws an error during persistence, the optimistic state is automatically rolled back.

## Collection Write Operations

Collections support three core write operations: `insert`, `update`, and `delete`. Each operation applies optimistic state immediately and triggers the corresponding operation handler.

### Insert

Add new items to a collection:

```typescript
// Insert a single item
todoCollection.insert({
  id: "1",
  text: "Buy groceries",
  completed: false
})

// Insert multiple items
todoCollection.insert([
  { id: "1", text: "Buy groceries", completed: false },
  { id: "2", text: "Walk dog", completed: false },
])

// Insert with metadata
todoCollection.insert(
  { id: "1", text: "Custom item", completed: false },
  { metadata: { source: "import" } }
)

// Insert without optimistic updates
todoCollection.insert(
  { id: "1", text: "Server-validated item", completed: false },
  { optimistic: false }
)
```

**Returns**: A `Transaction` object that you can use to track the mutation's lifecycle.

### Update

Modify existing items using an immutable draft pattern:

```typescript
// Update a single item
todoCollection.update(todo.id, (draft) => {
  draft.completed = true
})

// Update multiple items
todoCollection.update([todo1.id, todo2.id], (drafts) => {
  drafts.forEach((draft) => {
    draft.completed = true
  })
})

// Update with metadata
todoCollection.update(
  todo.id,
  { metadata: { reason: "user update" } },
  (draft) => {
    draft.text = "Updated text"
  }
)

// Update without optimistic updates
todoCollection.update(
  todo.id,
  { optimistic: false },
  (draft) => {
    draft.status = "server-validated"
  }
)
```

**Parameters**:
- `key` or `keys`: The item key(s) to update
- `options` (optional): Configuration object with `metadata` and/or `optimistic` flags
- `updater`: Function that receives a draft to mutate

**Returns**: A `Transaction` object that you can use to track the mutation's lifecycle.

> [!IMPORTANT]
> The `updater` function uses an Immer-like pattern to capture changes as immutable updates. You must not reassign the draft parameter itself—only mutate its properties.

### Delete

Remove items from a collection:

```typescript
// Delete a single item
todoCollection.delete(todo.id)

// Delete multiple items
todoCollection.delete([todo1.id, todo2.id])

// Delete with metadata
todoCollection.delete(todo.id, {
  metadata: { reason: "completed" }
})

// Delete without optimistic updates
todoCollection.delete(todo.id, { optimistic: false })
```

**Parameters**:
- `key` or `keys`: The item key(s) to delete
- `options` (optional): Configuration object with `metadata` and/or `optimistic` flags

**Returns**: A `Transaction` object that you can use to track the mutation's lifecycle.

## Operation Handlers

Operation handlers are functions you provide when creating a collection that handle persisting mutations to your backend. Each collection can define three optional handlers: `onInsert`, `onUpdate`, and `onDelete`.

### Handler Signature

All operation handlers receive an object with the following properties:

```typescript
type OperationHandler = (params: {
  transaction: Transaction
  collection: Collection
}) => Promise<any> | any
```

The `transaction` object contains:
- `mutations`: Array of mutation objects, each with:
  - `collection`: The collection being mutated
  - `type`: The mutation type (`'insert'`, `'update'`, or `'delete'`)
  - `original`: The original item (for updates and deletes)
  - `modified`: The modified item (for inserts and updates)
  - `changes`: The changes object (for updates)
  - `key`: The item key
  - `metadata`: Optional metadata attached to the mutation

### Defining Operation Handlers

Define handlers when creating a collection:

```typescript
const todoCollection = createCollection({
  id: "todos",
  // ... other options

  onInsert: async ({ transaction }) => {
    await Promise.all(
      transaction.mutations.map((mutation) =>
        api.todos.create(mutation.modified)
      )
    )
  },

  onUpdate: async ({ transaction }) => {
    await Promise.all(
      transaction.mutations.map((mutation) =>
        api.todos.update(mutation.original.id, mutation.changes)
      )
    )
  },

  onDelete: async ({ transaction }) => {
    await Promise.all(
      transaction.mutations.map((mutation) =>
        api.todos.delete(mutation.original.id)
      )
    )
  },
})
```

> [!IMPORTANT]
> Operation handlers must not resolve until the server changes have synced back to the collection. Different collection types provide different patterns to ensure this happens correctly.

### Collection-Specific Handler Patterns

Different collection types have specific patterns for their handlers:

**QueryCollection** - automatically refetches after handler completes:
```typescript
onUpdate: async ({ transaction }) => {
  await Promise.all(
    transaction.mutations.map((mutation) =>
      api.todos.update(mutation.original.id, mutation.changes)
    )
  )
  // Automatic refetch happens after handler completes
}
```

**ElectricCollection** - return txid(s) to track sync:
```typescript
onUpdate: async ({ transaction }) => {
  const txids = await Promise.all(
    transaction.mutations.map(async (mutation) => {
      const response = await api.todos.update(mutation.original.id, mutation.changes)
      return response.txid
    })
  )
  return { txid: txids }
}
```

### Generic Mutation Functions

You can define a single mutation function for your entire app:

```typescript
import type { MutationFn } from "@tanstack/react-db"

const mutationFn: MutationFn = async ({ transaction }) => {
  const response = await api.mutations.batch(transaction.mutations)

  if (!response.ok) {
    throw new Error(`HTTP Error: ${response.status}`)
  }
}

// Use in collections
const todoCollection = createCollection({
  id: "todos",
  onInsert: mutationFn,
  onUpdate: mutationFn,
  onDelete: mutationFn,
})
```

### Schema Validation in Mutation Handlers

When a schema is configured for a collection, TanStack DB automatically validates and transforms data during mutations. The mutation handlers receive the **transformed data** (TOutput), not the raw input.

```typescript
const todoSchema = z.object({
  id: z.string(),
  text: z.string(),
  created_at: z.string().transform(val => new Date(val))  // TInput: string, TOutput: Date
})

const collection = createCollection({
  schema: todoSchema,
  onInsert: async ({ transaction }) => {
    const item = transaction.mutations[0].modified

    // item.created_at is already a Date object (TOutput)
    console.log(item.created_at instanceof Date)  // true

    // If your API needs a string, serialize it
    await api.todos.create({
      ...item,
      created_at: item.created_at.toISOString()  // Date → string
    })
  }
})

// User provides string (TInput)
collection.insert({
  id: "1",
  text: "Task",
  created_at: "2024-01-01T00:00:00Z"
})
```

**Key points:**
- Schema validation happens **before** mutation handlers are called
- Handlers receive **TOutput** (transformed data)
- If your backend needs a different format, serialize in the handler
- Schema validation errors throw `SchemaValidationError` before handlers run

For comprehensive documentation on schema validation and transformations, see the [Schemas guide](./schemas.md).

## Creating Custom Actions

For more complex mutation patterns, use `createOptimisticAction` to create custom actions with full control over the mutation lifecycle.

### Basic Action

Create an action that combines mutation logic with persistence:

```tsx
import { createOptimisticAction } from "@tanstack/react-db"

const addTodo = createOptimisticAction<string>({
  onMutate: (text) => {
    // Apply optimistic state
    todoCollection.insert({
      id: crypto.randomUUID(),
      text,
      completed: false,
    })
  },
  mutationFn: async (text, params) => {
    // Persist to backend
    const response = await fetch("/api/todos", {
      method: "POST",
      body: JSON.stringify({ text, completed: false }),
    })
    const result = await response.json()

    // Wait for sync back
    await todoCollection.utils.refetch()

    return result
  },
})

// Use in components
const Todo = () => {
  const handleClick = () => {
    addTodo("🔥 Make app faster")
  }

  return <Button onClick={handleClick} />
}
```

### Type-Safe Actions with Schema Validation

For better type safety and runtime validation, you can use schema validation libraries like Zod, Valibot, or others. Here's an example using Zod:

```tsx
import { createOptimisticAction } from "@tanstack/react-db"
import { z } from "zod"

// Define a schema for the action parameters
const addTodoSchema = z.object({
  text: z.string().min(1, "Todo text cannot be empty"),
  priority: z.enum(["low", "medium", "high"]).optional(),
})

// Use the schema's inferred type for the generic
const addTodo = createOptimisticAction<z.infer<typeof addTodoSchema>>({
  onMutate: (params) => {
    // Validate parameters at runtime
    const validated = addTodoSchema.parse(params)

    // Apply optimistic state
    todoCollection.insert({
      id: crypto.randomUUID(),
      text: validated.text,
      priority: validated.priority ?? "medium",
      completed: false,
    })
  },
  mutationFn: async (params) => {
    // Parameters are already validated
    const validated = addTodoSchema.parse(params)

    const response = await fetch("/api/todos", {
      method: "POST",
      body: JSON.stringify({
        text: validated.text,
        priority: validated.priority ?? "medium",
        completed: false,
      }),
    })
    const result = await response.json()

    await todoCollection.utils.refetch()
    return result
  },
})

// Use with type-safe parameters
const Todo = () => {
  const handleClick = () => {
    addTodo({
      text: "🔥 Make app faster",
      priority: "high",
    })
  }

  return <Button onClick={handleClick} />
}
```

This pattern works with any validation library (Zod, Valibot, Yup, etc.) and provides:
- ✅ Runtime validation of parameters
- ✅ Type safety from inferred types
- ✅ Clear error messages for invalid inputs
- ✅ Single source of truth for parameter shape

### Complex Multi-Collection Actions

Actions can mutate multiple collections:

```tsx
const createProject = createOptimisticAction<{
  name: string
  ownerId: string
}>({
  onMutate: ({ name, ownerId }) => {
    const projectId = crypto.randomUUID()

    // Insert project
    projectCollection.insert({
      id: projectId,
      name,
      ownerId,
      createdAt: new Date(),
    })

    // Update user's project count
    userCollection.update(ownerId, (draft) => {
      draft.projectCount += 1
    })
  },
  mutationFn: async ({ name, ownerId }) => {
    const response = await api.projects.create({ name, ownerId })

    // Wait for both collections to sync
    await Promise.all([
      projectCollection.utils.refetch(),
      userCollection.utils.refetch(),
    ])

    return response
  },
})
```

### Action Parameters

The `mutationFn` receives additional parameters for advanced use cases:

```tsx
const updateTodo = createOptimisticAction<{
  id: string
  changes: Partial<Todo>
}>({
  onMutate: ({ id, changes }) => {
    todoCollection.update(id, (draft) => {
      Object.assign(draft, changes)
    })
  },
  mutationFn: async ({ id, changes }, params) => {
    // params.transaction contains the transaction object
    // params.signal is an AbortSignal for cancellation

    const response = await api.todos.update(id, changes, {
      signal: params.signal,
    })

    await todoCollection.utils.refetch()
    return response
  },
})
```

## Manual Transactions

For maximum control over transaction lifecycles, create transactions manually using `createTransaction`. This approach allows you to batch multiple mutations, implement custom commit workflows, or create transactions that span multiple user interactions.

### Basic Manual Transaction

```ts
import { createTransaction } from "@tanstack/react-db"

const addTodoTx = createTransaction({
  autoCommit: false,
  mutationFn: async ({ transaction }) => {
    // Persist all mutations to backend
    await Promise.all(
      transaction.mutations.map((mutation) =>
        api.saveTodo(mutation.modified)
      )
    )
  },
})

// Apply first change
addTodoTx.mutate(() =>
  todoCollection.insert({
    id: "1",
    text: "First todo",
    completed: false
  })
)

// User reviews change...

// Apply another change
addTodoTx.mutate(() =>
  todoCollection.insert({
    id: "2",
    text: "Second todo",
    completed: false
  })
)

// User commits when ready (e.g., when they hit save)
addTodoTx.commit()
```

### Transaction Configuration

Manual transactions accept the following options:

```typescript
createTransaction({
  id?: string,              // Optional unique identifier for the transaction
  autoCommit?: boolean,     // Whether to automatically commit after mutate()
  mutationFn: MutationFn,   // Function to persist mutations
  metadata?: Record<string, unknown>, // Optional custom metadata
})
```

**autoCommit**:
- `true` (default): Transaction commits immediately after each `mutate()` call
- `false`: Transaction waits for explicit `commit()` call

### Transaction Methods

Manual transactions provide several methods:

```typescript
// Apply mutations within a transaction
tx.mutate(() => {
  collection.insert(item)
  collection.update(key, updater)
})

// Commit the transaction
await tx.commit()

// Manually rollback changes (e.g., user cancels a form)
// Note: Rollback happens automatically if mutationFn throws an error
tx.rollback()
```

### Multi-Step Workflows

Manual transactions excel at complex workflows:

```ts
const reviewTx = createTransaction({
  autoCommit: false,
  mutationFn: async ({ transaction }) => {
    await api.batchUpdate(transaction.mutations)
  },
})

// Step 1: User makes initial changes
reviewTx.mutate(() => {
  todoCollection.update(id1, (draft) => {
    draft.status = "reviewed"
  })
  todoCollection.update(id2, (draft) => {
    draft.status = "reviewed"
  })
})

// Step 2: Show preview to user...

// Step 3: User confirms or makes additional changes
reviewTx.mutate(() => {
  todoCollection.update(id3, (draft) => {
    draft.status = "reviewed"
  })
})

// Step 4: User commits all changes at once
await reviewTx.commit()
// OR user cancels
// reviewTx.rollback()
```

### Using with Local Collections

LocalOnly and LocalStorage collections require special handling when used with manual transactions. Unlike server-synced collections that have `onInsert`, `onUpdate`, and `onDelete` handlers automatically invoked, local collections need you to manually accept mutations by calling `utils.acceptMutations()` in your transaction's `mutationFn`.

#### Why This Is Needed

Local collections (LocalOnly and LocalStorage) don't participate in the standard mutation handler flow for manual transactions. They need an explicit call to persist changes made during `tx.mutate()`.

#### Basic Usage

```ts
import { createTransaction } from "@tanstack/react-db"
import { localOnlyCollectionOptions } from "@tanstack/react-db"

const formDraft = createCollection(
  localOnlyCollectionOptions({
    id: "form-draft",
    getKey: (item) => item.id,
  })
)

const tx = createTransaction({
  autoCommit: false,
  mutationFn: async ({ transaction }) => {
    // Make API call with the data first
    const draftData = transaction.mutations
      .filter((m) => m.collection === formDraft)
      .map((m) => m.modified)

    await api.saveDraft(draftData)

    // After API succeeds, accept and persist local collection mutations
    formDraft.utils.acceptMutations(transaction)
  },
})

// Apply mutations
tx.mutate(() => {
  formDraft.insert({ id: "1", field: "value" })
})

// Commit when ready
await tx.commit()
```

#### Combining Local and Server Collections

You can mix local and server collections in the same transaction:

```ts
const localSettings = createCollection(
  localStorageCollectionOptions({
    id: "user-settings",
    storageKey: "app-settings",
    getKey: (item) => item.id,
  })
)

const userProfile = createCollection(
  queryCollectionOptions({
    queryKey: ["profile"],
    queryFn: async () => api.profile.get(),
    getKey: (item) => item.id,
    onUpdate: async ({ transaction }) => {
      await api.profile.update(transaction.mutations[0].modified)
    },
  })
)

const tx = createTransaction({
  mutationFn: async ({ transaction }) => {
    // Handle server collection mutations explicitly in mutationFn
    await Promise.all(
      transaction.mutations
        .filter((m) => m.collection === userProfile)
        .map((m) => api.profile.update(m.modified))
    )

    // After server mutations succeed, accept local collection mutations
    localSettings.utils.acceptMutations(transaction)
  },
})

// Update both local and server data in one transaction
tx.mutate(() => {
  localSettings.update("theme", (draft) => {
    draft.mode = "dark"
  })
  userProfile.update("user-1", (draft) => {
    draft.name = "Updated Name"
  })
})

await tx.commit()
```

#### Transaction Ordering

**When to call `acceptMutations`** matters for transaction semantics:

**After API success (recommended for consistency):**
```ts
mutationFn: async ({ transaction }) => {
  await api.save(data)  // API call first
  localData.utils.acceptMutations(transaction)  // Persist after success
}
```

✅ **Pros**: If the API fails, local changes roll back too (all-or-nothing semantics)
❌ **Cons**: Local state won't reflect changes until API succeeds

**Before API call (for independent local state):**
```ts
mutationFn: async ({ transaction }) => {
  localData.utils.acceptMutations(transaction)  // Persist first
  await api.save(data)  // Then API call
}
```

✅ **Pros**: Local state persists immediately, regardless of API outcome
❌ **Cons**: API failure leaves local changes persisted (divergent state)

Choose based on whether your local data should be independent of or coupled to remote mutations.

#### Best Practices

- Always call `utils.acceptMutations()` for local collections in manual transactions
- Call `acceptMutations` **after** API success if you want transactional consistency
- Call `acceptMutations` **before** API calls if local state should persist regardless
- Filter mutations by collection if you need to process them separately
- Mix local and server collections freely in the same transaction

### Listening to Transaction Lifecycle

Monitor transaction state changes:

```typescript
const tx = createTransaction({
  autoCommit: false,
  mutationFn: async ({ transaction }) => {
    await api.persist(transaction.mutations)
  },
})

// Wait for transaction to complete
tx.isPersisted.promise.then(() => {
  console.log("Transaction persisted!")
})

// Check current state
console.log(tx.state) // 'pending', 'persisting', 'completed', or 'failed'
```

## Paced Mutations

Paced mutations provide fine-grained control over **when and how** mutations are persisted to your backend. Instead of persisting every mutation immediately, you can use timing strategies to batch, delay, or queue mutations based on your application's needs.

Powered by [TanStack Pacer](https://github.com/TanStack/pacer), paced mutations are ideal for scenarios like:
- **Auto-save forms** that wait for the user to stop typing
- **Slider controls** that need smooth updates without overwhelming the backend
- **Sequential workflows** where order matters and every mutation must persist

### Key Design

The fundamental difference between strategies is how they handle transactions:

**Debounce/Throttle**: Only one pending transaction (collecting mutations) and one persisting transaction (writing to backend) at a time. Multiple rapid mutations automatically merge together into a single transaction.

**Queue**: Each mutation creates a separate transaction, guaranteed to run in the order they're made (FIFO by default, configurable to LIFO). All mutations are guaranteed to persist.

### Available Strategies

| Strategy | Behavior | Best For |
|----------|----------|----------|
| **`debounceStrategy`** | Wait for inactivity before persisting. Only final state is saved. | Auto-save forms, search-as-you-type |
| **`throttleStrategy`** | Ensure minimum spacing between executions. Mutations between executions are merged. | Sliders, progress updates, analytics |
| **`queueStrategy`** | Each mutation becomes a separate transaction, processed sequentially in order (FIFO by default, configurable to LIFO). All mutations guaranteed to persist. | Sequential workflows, file uploads, rate-limited APIs |

### Debounce Strategy

The debounce strategy waits for a period of inactivity before persisting. This is perfect for auto-save scenarios where you want to wait until the user stops typing before saving their work.

```tsx
import { usePacedMutations, debounceStrategy } from "@tanstack/react-db"

function AutoSaveForm({ formId }: { formId: string }) {
  const mutate = usePacedMutations<{ field: string; value: string }>({
    onMutate: ({ field, value }) => {
      // Apply optimistic update immediately
      formCollection.update(formId, (draft) => {
        draft[field] = value
      })
    },
    mutationFn: async ({ transaction }) => {
      // Persist the final merged state to the backend
      await api.forms.save(transaction.mutations)
    },
    // Wait 500ms after the last change before persisting
    strategy: debounceStrategy({ wait: 500 }),
  })

  const handleChange = (field: string, value: string) => {
    // Multiple rapid changes merge into a single transaction
    mutate({ field, value })
  }

  return (
    <form>
      <input onChange={(e) => handleChange('title', e.target.value)} />
      <textarea onChange={(e) => handleChange('content', e.target.value)} />
    </form>
  )
}
```

**Key characteristics**:
- Timer resets on each mutation
- Only the final merged state persists
- Reduces backend writes significantly for rapid changes

### Throttle Strategy

The throttle strategy ensures a minimum spacing between executions. This is ideal for scenarios like sliders or progress updates where you want smooth, consistent updates without overwhelming your backend.

```tsx
import { usePacedMutations, throttleStrategy } from "@tanstack/react-db"

function VolumeSlider() {
  const mutate = usePacedMutations<number>({
    onMutate: (volume) => {
      // Apply optimistic update immediately
      settingsCollection.update('volume', (draft) => {
        draft.value = volume
      })
    },
    mutationFn: async ({ transaction }) => {
      await api.settings.updateVolume(transaction.mutations)
    },
    // Persist at most once every 200ms
    strategy: throttleStrategy({
      wait: 200,
      leading: true,   // Execute immediately on first call
      trailing: true,  // Execute after wait period if there were mutations
    }),
  })

  const handleVolumeChange = (volume: number) => {
    mutate(volume)
  }

  return (
    <input
      type="range"
      min={0}
      max={100}
      onChange={(e) => handleVolumeChange(Number(e.target.value))}
    />
  )
}
```

**Key characteristics**:
- Guarantees minimum spacing between persists
- Can execute on leading edge, trailing edge, or both
- Mutations between executions are merged

### Queue Strategy

The queue strategy creates a separate transaction for each mutation and processes them sequentially in order. Unlike debounce/throttle, **every mutation is guaranteed to persist**, making it ideal for workflows where you can't lose any operations.

```tsx
import { usePacedMutations, queueStrategy } from "@tanstack/react-db"

function FileUploader() {
  const mutate = usePacedMutations<File>({
    onMutate: (file) => {
      // Apply optimistic update immediately
      uploadCollection.insert({
        id: crypto.randomUUID(),
        file,
        status: 'pending',
      })
    },
    mutationFn: async ({ transaction }) => {
      // Each file upload is its own transaction
      const mutation = transaction.mutations[0]
      await api.files.upload(mutation.modified)
    },
    // Process each upload sequentially with 500ms between them
    strategy: queueStrategy({
      wait: 500,
      addItemsTo: 'back',    // FIFO: add to back of queue
      getItemsFrom: 'front', // FIFO: process from front of queue
    }),
  })

  const handleFileSelect = (files: FileList) => {
    // Each file creates its own transaction, queued for sequential processing
    Array.from(files).forEach((file) => {
      mutate(file)
    })
  }

  return <input type="file" multiple onChange={(e) => handleFileSelect(e.target.files!)} />
}
```

**Key characteristics**:
- Each mutation becomes its own transaction
- Processes sequentially in order (FIFO by default)
- Can configure to LIFO by setting `getItemsFrom: 'back'`
- All mutations guaranteed to persist
- Waits for each transaction to complete before starting the next

### Choosing a Strategy

Use this guide to pick the right strategy for your use case:

**Use `debounceStrategy` when:**
- You want to wait for the user to finish their action
- Only the final state matters (intermediate states can be discarded)
- You want to minimize backend writes
- Examples: auto-save forms, search-as-you-type, settings panels

**Use `throttleStrategy` when:**
- You want smooth, consistent updates at a controlled rate
- Some intermediate states should persist, but not all
- You need updates to feel responsive without overwhelming the backend
- Examples: volume sliders, progress bars, analytics tracking, live cursor position

**Use `queueStrategy` when:**
- Every mutation must persist (no operations can be lost)
- Order of operations matters
- You're working with a rate-limited API
- You need sequential processing with delays
- Examples: file uploads, batch operations, audit trails, multi-step wizards

### Using in React

The `usePacedMutations` hook makes it easy to use paced mutations in React components:

```tsx
import { usePacedMutations, debounceStrategy } from "@tanstack/react-db"

function MyComponent({ itemId }: { itemId: string }) {
  const mutate = usePacedMutations<number>({
    onMutate: (newValue) => {
      // Apply optimistic update immediately
      collection.update(itemId, (draft) => {
        draft.value = newValue
      })
    },
    mutationFn: async ({ transaction }) => {
      await api.save(transaction.mutations)
    },
    strategy: debounceStrategy({ wait: 500 }),
  })

  // Each mutate call returns a Transaction you can await
  const handleSave = async (newValue: number) => {
    const tx = mutate(newValue)

    // Optionally wait for persistence
    try {
      await tx.isPersisted.promise
      console.log('Saved successfully!')
    } catch (error) {
      console.error('Save failed:', error)
    }
  }

  return <button onClick={() => handleSave(42)}>Save</button>
}
```

The hook automatically memoizes the strategy and mutation function to prevent unnecessary recreations. You can also use `createPacedMutations` directly outside of React:

```ts
import { createPacedMutations, queueStrategy } from "@tanstack/db"

const mutate = createPacedMutations<{ id: string; changes: Partial<Item> }>({
  onMutate: ({ id, changes }) => {
    // Apply optimistic update immediately
    collection.update(id, (draft) => {
      Object.assign(draft, changes)
    })
  },
  mutationFn: async ({ transaction }) => {
    await api.save(transaction.mutations)
  },
  strategy: queueStrategy({ wait: 200 }),
})

// Use anywhere in your application
mutate({ id: '123', changes: { name: 'New Name' } })
```

### Understanding Queues and Hook Instances

**Each unique `usePacedMutations` hook call creates its own independent queue.** This is an important design decision that affects how you structure your mutations.

If you have multiple components calling `usePacedMutations` separately, each will have its own isolated queue:

```tsx
function EmailDraftEditor1({ draftId }: { draftId: string }) {
  // This creates Queue A
  const mutate = usePacedMutations({
    onMutate: (text) => {
      draftCollection.update(draftId, (draft) => {
        draft.text = text
      })
    },
    mutationFn: async ({ transaction }) => {
      await api.saveDraft(transaction.mutations)
    },
    strategy: debounceStrategy({ wait: 500 }),
  })

  return <textarea onChange={(e) => mutate(e.target.value)} />
}

function EmailDraftEditor2({ draftId }: { draftId: string }) {
  // This creates Queue B (separate from Queue A)
  const mutate = usePacedMutations({
    onMutate: (text) => {
      draftCollection.update(draftId, (draft) => {
        draft.text = text
      })
    },
    mutationFn: async ({ transaction }) => {
      await api.saveDraft(transaction.mutations)
    },
    strategy: debounceStrategy({ wait: 500 }),
  })

  return <textarea onChange={(e) => mutate(e.target.value)} />
}
```

In this example, mutations from `EmailDraftEditor1` and `EmailDraftEditor2` will be queued and processed **independently**. They won't share the same debounce timer or queue.

**To share the same queue across multiple components**, create a single `createPacedMutations` instance and use it everywhere:

```tsx
// Create a single shared instance
import { createPacedMutations, debounceStrategy } from "@tanstack/db"

export const mutateDraft = createPacedMutations<{ draftId: string; text: string }>({
  onMutate: ({ draftId, text }) => {
    draftCollection.update(draftId, (draft) => {
      draft.text = text
    })
  },
  mutationFn: async ({ transaction }) => {
    await api.saveDraft(transaction.mutations)
  },
  strategy: debounceStrategy({ wait: 500 }),
})

// Now both components share the same queue
function EmailDraftEditor1({ draftId }: { draftId: string }) {
  return <textarea onChange={(e) => mutateDraft({ draftId, text: e.target.value })} />
}

function EmailDraftEditor2({ draftId }: { draftId: string }) {
  return <textarea onChange={(e) => mutateDraft({ draftId, text: e.target.value })} />
}
```

With this approach, all mutations from both components share the same debounce timer and queue, ensuring they're processed in the correct order with a single debounce implementation.

**Key takeaways:**

- Each `usePacedMutations()` call = unique queue
- Each `createPacedMutations()` call = unique queue
- To share a queue: create one instance and import it everywhere you need it
- Shared queues ensure mutations from different places are ordered correctly

## Mutation Merging

When multiple mutations operate on the same item within a transaction, TanStack DB intelligently merges them to:
- **Reduce network traffic**: Fewer mutations sent to the server
- **Preserve user intent**: Final state matches what user expects
- **Maintain UI consistency**: Local state always reflects user actions

The merging behavior follows a truth table based on the mutation types:

| Existing → New      | Result    | Description                                       |
| ------------------- | --------- | ------------------------------------------------- |
| **insert + update** | `insert`  | Keeps insert type, merges changes, empty original |
| **insert + delete** | _removed_ | Mutations cancel each other out                   |
| **update + delete** | `delete`  | Delete dominates                                  |
| **update + update** | `update`  | Union changes, keep first original                |

> [!NOTE]
> Attempting to insert or delete the same item multiple times within a transaction will throw an error.

## Controlling Optimistic Behavior

By default, all mutations apply optimistic updates immediately to provide instant feedback. However, you can disable this behavior when you need to wait for server confirmation before applying changes locally.

### When to Disable Optimistic Updates

Consider using `optimistic: false` when:

- **Complex server-side processing**: Operations that depend on server-side generation (e.g., cascading foreign keys, computed fields)
- **Validation requirements**: Operations where backend validation might reject the change
- **Confirmation workflows**: Deletes where UX should wait for confirmation before removing data
- **Batch operations**: Large operations where optimistic rollback would be disruptive

### Behavior Differences

**`optimistic: true` (default)**:
- Immediately applies mutation to the local store
- Provides instant UI feedback
- Requires rollback if server rejects the mutation
- Best for simple, predictable operations

**`optimistic: false`**:
- Does not modify local store until server confirms
- No immediate UI feedback, but no rollback needed
- UI updates only after successful server response
- Best for complex or validation-heavy operations

### Using Non-Optimistic Mutations

```typescript
// Critical deletion that needs confirmation
const handleDeleteAccount = () => {
  userCollection.delete(userId, { optimistic: false })
}

// Server-generated data
const handleCreateInvoice = () => {
  // Server generates invoice number, tax calculations, etc.
  invoiceCollection.insert(invoiceData, { optimistic: false })
}

// Mixed approach in same transaction
tx.mutate(() => {
  // Instant UI feedback for simple change
  todoCollection.update(todoId, (draft) => {
    draft.completed = true
  })

  // Wait for server confirmation for complex change
  auditCollection.insert(auditRecord, { optimistic: false })
})
```

### Waiting for Persistence

A common pattern with `optimistic: false` is to wait for the mutation to complete before navigating or showing success feedback:

```typescript
const handleCreatePost = async (postData) => {
  // Insert without optimistic updates
  const tx = postsCollection.insert(postData, { optimistic: false })

  try {
    // Wait for write to server and sync back to complete
    await tx.isPersisted.promise

    // Server write and sync back were successful
    navigate(`/posts/${postData.id}`)
  } catch (error) {
    // Show error notification
    toast.error("Failed to create post: " + error.message)
  }
}

// Works with updates and deletes too
const handleUpdateTodo = async (todoId, changes) => {
  const tx = todoCollection.update(
    todoId,
    { optimistic: false },
    (draft) => Object.assign(draft, changes)
  )

  try {
    await tx.isPersisted.promise
    navigate("/todos")
  } catch (error) {
    toast.error("Failed to update todo: " + error.message)
  }
}
```

## Transaction States

Transactions progress through the following states during their lifecycle:

1. **`pending`**: Initial state when a transaction is created and optimistic mutations can be applied
2. **`persisting`**: Transaction is being persisted to the backend
3. **`completed`**: Transaction has been successfully persisted and any backend changes have been synced back
4. **`failed`**: An error was thrown while persisting or syncing back the transaction

### Monitoring Transaction State

```typescript
const tx = todoCollection.update(todoId, (draft) => {
  draft.completed = true
})

// Check current state
console.log(tx.state) // 'pending'

// Wait for specific states
await tx.isPersisted.promise
console.log(tx.state) // 'completed' or 'failed'

// Handle errors
try {
  await tx.isPersisted.promise
  console.log("Success!")
} catch (error) {
  console.log("Failed:", error)
}
```

### State Transitions

The normal flow is: `pending` → `persisting` → `completed`

If an error occurs: `pending` → `persisting` → `failed`

Failed transactions automatically rollback their optimistic state.

## Handling Temporary IDs

When inserting new items into collections where the server generates the final ID, you'll need to handle the transition from temporary to real IDs carefully to avoid UI issues and operation failures.

### The Problem

When you insert an item with a temporary ID, the optimistic object is eventually replaced by the synced object with its real server-generated ID. This can cause two issues:

1. **UI Flicker**: Your UI framework may unmount and remount components when the key changes from temporary to real ID
2. **Subsequent Operations**: Operations like delete may fail if they try to use the temporary ID before the real ID syncs back

```tsx
// Generate temporary ID (e.g., negative number)
const tempId = -(Math.floor(Math.random() * 1000000) + 1)

// Insert with temporary ID
todoCollection.insert({
  id: tempId,
  text: "New todo",
  completed: false
})

// Problem 1: UI may re-render when tempId is replaced with real ID
// Problem 2: Trying to delete before sync completes will use tempId
todoCollection.delete(tempId) // May 404 on backend
```

### Solution 1: Use Client-Generated UUIDs

If your backend supports client-generated IDs, use UUIDs to eliminate the temporary ID problem entirely:

```tsx
// Generate UUID on client
const id = crypto.randomUUID()

todoCollection.insert({
  id,
  text: "New todo",
  completed: false
})

// No flicker - the ID is stable
// Subsequent operations work immediately
todoCollection.delete(id) // Works with the same ID
```

This is the cleanest approach when your backend supports it, as the ID never changes.

### Solution 2: Wait for Persistence or Use Non-Optimistic Inserts

Wait for the mutation to persist before allowing subsequent operations, or use non-optimistic inserts to avoid showing the item until the real ID is available:

```tsx
const handleCreateTodo = async (text: string) => {
  const tempId = -Math.floor(Math.random() * 1000000) + 1

  const tx = todoCollection.insert({
    id: tempId,
    text,
    completed: false
  })

  // Wait for persistence to complete
  await tx.isPersisted.promise

  // Now we have the real ID from the server
  // Subsequent operations will use the real ID
}

// Disable delete buttons until persisted
const TodoItem = ({ todo, isPersisted }: { todo: Todo, isPersisted: boolean }) => {
  return (
    <div>
      {todo.text}
      <button
        onClick={() => todoCollection.delete(todo.id)}
        disabled={!isPersisted}
      >
        Delete
      </button>
    </div>
  )
}
```

### Solution 3: Maintain a View Key Mapping

To avoid UI flicker while keeping optimistic updates, maintain a separate mapping from IDs (both temporary and real) to stable view keys:

```tsx
// Create a mapping API
const idToViewKey = new Map<number | string, string>()

function getViewKey(id: number | string): string {
  if (!idToViewKey.has(id)) {
    idToViewKey.set(id, crypto.randomUUID())
  }
  return idToViewKey.get(id)!
}

function linkIds(tempId: number, realId: number) {
  const viewKey = getViewKey(tempId)
  idToViewKey.set(realId, viewKey)
}

// Configure collection to link IDs when real ID comes back
const todoCollection = createCollection({
  id: "todos",
  // ... other options
  onInsert: async ({ transaction }) => {
    const mutation = transaction.mutations[0]
    const tempId = mutation.modified.id

    // Create todo on server and get real ID back
    const response = await api.todos.create({
      text: mutation.modified.text,
      completed: mutation.modified.completed,
    })
    const realId = response.id

    // Link temp ID to same view key as real ID
    linkIds(tempId, realId)

    // Wait for sync back
    await todoCollection.utils.refetch()
  },
})

// When inserting with temp ID
const tempId = -Math.floor(Math.random() * 1000000) + 1
const viewKey = getViewKey(tempId) // Creates and stores mapping

todoCollection.insert({
  id: tempId,
  text: "New todo",
  completed: false
})

// Use view key for rendering
const TodoList = () => {
  const { data: todos } = useLiveQuery((q) =>
    q.from({ todo: todoCollection })
  )

  return (
    <ul>
      {todos.map((todo) => (
        <li key={getViewKey(todo.id)}> {/* Stable key */}
          {todo.text}
        </li>
      ))}
    </ul>
  )
}
```

This pattern maintains a stable key throughout the temporary → real ID transition, preventing your UI framework from unmounting and remounting the component. The view key is stored outside the collection items, so you don't need to add extra fields to your data model.

### Best Practices

1. **Use UUIDs when possible**: Client-generated UUIDs eliminate the temporary ID problem
2. **Generate temporary IDs deterministically**: Use negative numbers or a specific pattern to distinguish temporary IDs from real ones
3. **Disable operations on temporary items**: Disable delete/update buttons until persistence completes
4. **Maintain view key mappings**: Create a mapping between IDs and stable view keys for rendering

> [!NOTE]
> There's an [open issue](https://github.com/TanStack/db/issues/19) to add better built-in support for temporary ID handling in TanStack DB. This would automate the view key pattern and make it easier to work with server-generated IDs.


---


---
title: Schemas
id: schemas
---

# Schema Validation and Type Transformations

TanStack DB uses schemas to ensure your data is valid and type-safe throughout your application.

## What You'll Learn

This guide covers:
- How schema validation works in TanStack DB
- Understanding TInput and TOutput types
- Common patterns: validation, transformations, and defaults
- Error handling and best practices

## Quick Example

Schemas catch invalid data from optimistic mutations before it enters your collection:

```typescript
import { z } from 'zod'
import { createCollection } from '@tanstack/react-db'
import { queryCollectionOptions } from '@tanstack/query-db-collection'

const todoSchema = z.object({
  id: z.string(),
  text: z.string().min(1, "Text is required"),
  completed: z.boolean(),
  priority: z.number().min(0).max(5)
})

const collection = createCollection(
  queryCollectionOptions({
    schema: todoSchema,
    queryKey: ['todos'],
    queryFn: async () => api.todos.getAll(),
    getKey: (item) => item.id,
    // ...
  })
)

// Invalid data throws SchemaValidationError
collection.insert({
  id: "1",
  text: "",  // ❌ Too short
  completed: "yes",  // ❌ Wrong type
  priority: 10  // ❌ Out of range
})
// Error: Validation failed with 3 issues

// Valid data works
collection.insert({
  id: "1",
  text: "Buy groceries",  // ✅
  completed: false,  // ✅
  priority: 2  // ✅
})
```

Schemas also enable advanced features like type transformations and defaults:

```typescript
const todoSchema = z.object({
  id: z.string(),
  text: z.string().min(1),
  completed: z.boolean().default(false),  // Auto-fill missing values
  created_at: z.string().transform(val => new Date(val))  // Convert types
})

collection.insert({
  id: "1",
  text: "Buy groceries",
  created_at: "2024-01-01T00:00:00Z"  // String in
  // completed auto-filled with false
})

const todo = collection.get("1")
console.log(todo.created_at.getFullYear())  // Date object out!
```

## Supported Schema Libraries

TanStack DB supports any [StandardSchema](https://standardschema.dev) compatible library:
- [Zod](https://zod.dev)
- [Valibot](https://valibot.dev)
- [ArkType](https://arktype.io)
- [Effect Schema](https://effect.website/docs/schema/introduction/)

Examples in this guide use Zod, but patterns apply to all libraries.

---

## Core Concepts: TInput vs TOutput

Understanding TInput and TOutput is key to working effectively with schemas in TanStack DB.

> **Important:** Schemas validate **client changes only** - data you insert or update via `collection.insert()` and `collection.update()`. They do not automatically validate data loaded from your server or sync layer. If you need to validate server data, you must do so explicitly in your integration layer.

### What are TInput and TOutput?

When you define a schema with transformations, it has two types:

- **TInput**: The type users provide when calling `insert()` or `update()`
- **TOutput**: The type stored in the collection and returned from queries

```typescript
const todoSchema = z.object({
  id: z.string(),
  text: z.string(),
  created_at: z.string().transform(val => new Date(val))
})

// TInput type:  { id: string, text: string, created_at: string }
// TOutput type: { id: string, text: string, created_at: Date }
```

The schema acts as a **boundary** that transforms TInput → TOutput.

### Critical Design Principle: TInput Must Be a Superset of TOutput

When using transformations, **TInput must accept all values that TOutput contains**. This is essential for updates to work correctly.

Here's why: when you call `collection.update(id, (draft) => {...})`, the `draft` parameter is typed as `TInput` but contains data that's already been transformed to `TOutput`. For this to work without complex type gymnastics, your schema must accept both the input format AND the output format.

```typescript
// ❌ BAD: TInput only accepts strings
const schema = z.object({
  created_at: z.string().transform(val => new Date(val))
})
// TInput:  { created_at: string }
// TOutput: { created_at: Date }
// Problem: draft.created_at is a Date, but TInput only accepts string!

// ✅ GOOD: TInput accepts both string and Date (superset of TOutput)
const schema = z.object({
  created_at: z.union([z.string(), z.date()])
    .transform(val => typeof val === 'string' ? new Date(val) : val)
})
// TInput:  { created_at: string | Date }
// TOutput: { created_at: Date }
// Success: draft.created_at can be a Date because TInput accepts Date!
```

**Rule of thumb:** If your schema transforms type A to type B, use `z.union([A, B])` to ensure TInput accepts both.

### Why This Matters

**All data in your collection is TOutput:**
- Data stored in the collection
- Data returned from queries
- Data in `PendingMutation.modified`
- Data in mutation handlers

```typescript
const collection = createCollection({
  schema: todoSchema,
  onInsert: async ({ transaction }) => {
    const item = transaction.mutations[0].modified

    // item is TOutput
    console.log(item.created_at instanceof Date)  // true

    // If your API needs a string, serialize it
    await api.todos.create({
      ...item,
      created_at: item.created_at.toISOString()  // Date → string
    })
  }
})

// User provides TInput
collection.insert({
  id: "1",
  text: "Task",
  created_at: "2024-01-01T00:00:00Z"  // string
})

// Collection stores and returns TOutput
const todo = collection.get("1")
console.log(todo.created_at.getFullYear())  // It's a Date!
```

---

## Validation Patterns

Schemas provide powerful validation to ensure data quality.

### Basic Type Validation

```typescript
const userSchema = z.object({
  id: z.string(),
  name: z.string(),
  age: z.number(),
  email: z.string().email(),
  active: z.boolean()
})

collection.insert({
  id: "1",
  name: "Alice",
  age: "25",  // ❌ Wrong type - expects number
  email: "not-an-email",  // ❌ Invalid email format
  active: true
})
// Throws SchemaValidationError
```

### String Constraints

```typescript
const productSchema = z.object({
  id: z.string(),
  name: z.string().min(3, "Name must be at least 3 characters"),
  sku: z.string().length(8, "SKU must be exactly 8 characters"),
  description: z.string().max(500, "Description too long"),
  url: z.string().url("Must be a valid URL")
})
```

### Number Constraints

```typescript
const orderSchema = z.object({
  id: z.string(),
  quantity: z.number()
    .int("Must be a whole number")
    .positive("Must be greater than 0"),
  price: z.number()
    .min(0.01, "Price must be at least $0.01")
    .max(999999.99, "Price too high"),
  discount: z.number()
    .min(0)
    .max(100)
})
```

### Enum Validation

```typescript
const taskSchema = z.object({
  id: z.string(),
  status: z.enum(['todo', 'in-progress', 'done']),
  priority: z.enum(['low', 'medium', 'high', 'urgent'])
})

collection.insert({
  id: "1",
  status: "completed",  // ❌ Not in enum
  priority: "medium"  // ✅
})
```

### Optional and Nullable Fields

```typescript
const personSchema = z.object({
  id: z.string(),
  name: z.string(),
  nickname: z.string().optional(),  // Can be omitted
  middleName: z.string().nullable(),  // Can be null
  bio: z.string().optional().nullable()  // Can be omitted OR null
})

// All valid:
collection.insert({ id: "1", name: "Alice" })  // nickname omitted
collection.insert({ id: "2", name: "Bob", middleName: null })
collection.insert({ id: "3", name: "Carol", bio: null })
```

### Array Validation

```typescript
const postSchema = z.object({
  id: z.string(),
  title: z.string(),
  tags: z.array(z.string()).min(1, "At least one tag required"),
  likes: z.array(z.number()).max(1000)
})

collection.insert({
  id: "1",
  title: "My Post",
  tags: [],  // ❌ Need at least one
  likes: [1, 2, 3]
})
```

### Custom Validation

```typescript
const userSchema = z.object({
  id: z.string(),
  username: z.string()
    .min(3)
    .refine(
      (val) => /^[a-zA-Z0-9_]+$/.test(val),
      "Username can only contain letters, numbers, and underscores"
    ),
  password: z.string()
    .min(8)
    .refine(
      (val) => /[A-Z]/.test(val) && /[0-9]/.test(val),
      "Password must contain at least one uppercase letter and one number"
    )
})
```

### Cross-Field Validation

```typescript
const dateRangeSchema = z.object({
  id: z.string(),
  start_date: z.string(),
  end_date: z.string()
}).refine(
  (data) => new Date(data.end_date) > new Date(data.start_date),
  "End date must be after start date"
)
```

---

## Transformation Patterns

Schemas can transform data as it enters your collection.

### String to Date

The most common transformation - convert ISO strings to Date objects:

```typescript
const eventSchema = z.object({
  id: z.string(),
  name: z.string(),
  start_time: z.string().transform(val => new Date(val))
})

collection.insert({
  id: "1",
  name: "Conference",
  start_time: "2024-06-15T10:00:00Z"  // TInput: string
})

const event = collection.get("1")
console.log(event.start_time.getFullYear())  // TOutput: Date
```

### String to Number

```typescript
const formSchema = z.object({
  id: z.string(),
  quantity: z.string().transform(val => parseInt(val, 10)),
  price: z.string().transform(val => parseFloat(val))
})

collection.insert({
  id: "1",
  quantity: "42",  // String from form input
  price: "19.99"
})

const item = collection.get("1")
console.log(typeof item.quantity)  // "number"
```

### JSON String to Object

```typescript
const configSchema = z.object({
  id: z.string(),
  settings: z.string().transform(val => JSON.parse(val))
})

collection.insert({
  id: "1",
  settings: '{"theme":"dark","notifications":true}'  // JSON string
})

const config = collection.get("1")
console.log(config.settings.theme)  // "dark" (parsed object)
```

### Computed Fields

```typescript
const userSchema = z.object({
  id: z.string(),
  first_name: z.string(),
  last_name: z.string()
}).transform(data => ({
  ...data,
  full_name: `${data.first_name} ${data.last_name}`  // Computed
}))

collection.insert({
  id: "1",
  first_name: "John",
  last_name: "Doe"
})

const user = collection.get("1")
console.log(user.full_name)  // "John Doe"
```

### String to Enum

```typescript
const orderSchema = z.object({
  id: z.string(),
  status: z.string().transform(val =>
    val.toUpperCase() as 'PENDING' | 'SHIPPED' | 'DELIVERED'
  )
})
```

### Sanitization

```typescript
const commentSchema = z.object({
  id: z.string(),
  text: z.string().transform(val => val.trim()),  // Remove whitespace
  username: z.string().transform(val => val.toLowerCase())  // Normalize
})
```

### Complex Transformations

```typescript
const productSchema = z.object({
  id: z.string(),
  name: z.string(),
  price_cents: z.number()
}).transform(data => ({
  ...data,
  price_dollars: data.price_cents / 100,  // Add computed field
  display_price: `$${(data.price_cents / 100).toFixed(2)}`  // Formatted
}))
```

---

## Default Values

Schemas can automatically provide default values for missing fields.

### Literal Defaults

```typescript
const todoSchema = z.object({
  id: z.string(),
  text: z.string(),
  completed: z.boolean().default(false),
  priority: z.number().default(0),
  tags: z.array(z.string()).default([])
})

collection.insert({
  id: "1",
  text: "Buy groceries"
  // completed, priority, and tags filled automatically
})

const todo = collection.get("1")
console.log(todo.completed)  // false
console.log(todo.priority)   // 0
console.log(todo.tags)       // []
```

### Function Defaults

Generate defaults dynamically:

```typescript
const postSchema = z.object({
  id: z.string(),
  title: z.string(),
  created_at: z.date().default(() => new Date()),
  view_count: z.number().default(0),
  slug: z.string().default(() => crypto.randomUUID())
})

collection.insert({
  id: "1",
  title: "My First Post"
  // created_at, view_count, and slug generated automatically
})
```

### Conditional Defaults

```typescript
const userSchema = z.object({
  id: z.string(),
  username: z.string(),
  role: z.enum(['user', 'admin']).default('user'),
  permissions: z.array(z.string()).default(['read'])
})
```

### Complex Defaults

```typescript
const eventSchema = z.object({
  id: z.string(),
  name: z.string(),
  metadata: z.record(z.unknown()).default(() => ({
    created_by: 'system',
    version: 1
  }))
})
```

### Combining Defaults with Transformations

```typescript
const todoSchema = z.object({
  id: z.string(),
  text: z.string(),
  completed: z.boolean().default(false),
  created_at: z.string()
    .default(() => new Date().toISOString())
    .transform(val => new Date(val))
})

collection.insert({
  id: "1",
  text: "Task"
  // completed defaults to false
  // created_at defaults to current time, then transforms to Date
})
```

---

## Handling Timestamps

When working with timestamps, you typically want automatic creation dates rather than transforming user input.

### Use Defaults for Timestamps

For `created_at` and `updated_at` fields, use defaults to automatically generate timestamps:

```typescript
const todoSchema = z.object({
  id: z.string(),
  text: z.string(),
  completed: z.boolean().default(false),
  created_at: z.date().default(() => new Date()),
  updated_at: z.date().default(() => new Date())
})

// Timestamps generated automatically
collection.insert({
  id: "1",
  text: "Buy groceries"
  // created_at and updated_at filled automatically
})

// Update timestamps
collection.update("1", (draft) => {
  draft.text = "Buy groceries and milk"
  draft.updated_at = new Date()
})
```

### Accepting Date Input from External Sources

If you're accepting date input from external sources (forms, APIs), you must use union types to accept both strings and Date objects. This ensures TInput is a superset of TOutput:

```typescript
const eventSchema = z.object({
  id: z.string(),
  name: z.string(),
  scheduled_for: z.union([
    z.string(),  // Accept ISO string from form input (part of TInput)
    z.date()     // Accept Date from existing data (TOutput) or programmatic input
  ]).transform(val =>
    typeof val === 'string' ? new Date(val) : val
  )
})
// TInput:  { scheduled_for: string | Date }
// TOutput: { scheduled_for: Date }
// ✅ TInput is a superset of TOutput (accepts both string and Date)

// Works with string input (new data)
collection.insert({
  id: "1",
  name: "Meeting",
  scheduled_for: "2024-12-31T15:00:00Z"  // From form input
})

// Works with Date input (programmatic)
collection.insert({
  id: "2",
  name: "Workshop",
  scheduled_for: new Date()
})

// Updates work - scheduled_for is already a Date, and TInput accepts Date
collection.update("1", (draft) => {
  draft.name = "Updated Meeting"
  // draft.scheduled_for is a Date and can be used or modified
})
```

---

## Error Handling

When validation fails, TanStack DB throws a `SchemaValidationError` with detailed information.

### Basic Error Handling

```typescript
import { SchemaValidationError } from '@tanstack/db'

try {
  collection.insert({
    id: "1",
    email: "not-an-email",
    age: -5
  })
} catch (error) {
  if (error instanceof SchemaValidationError) {
    console.log(error.type)     // 'insert' or 'update'
    console.log(error.message)  // "Validation failed with 2 issues"
    console.log(error.issues)   // Array of validation issues
  }
}
```

### Error Structure

```typescript
error.issues = [
  {
    path: ['email'],
    message: 'Invalid email address'
  },
  {
    path: ['age'],
    message: 'Number must be greater than 0'
  }
]
```

### Displaying Errors in UI

```typescript
const handleSubmit = async (data: unknown) => {
  try {
    collection.insert(data)
  } catch (error) {
    if (error instanceof SchemaValidationError) {
      // Show errors by field
      error.issues.forEach(issue => {
        const fieldName = issue.path?.join('.') || 'unknown'
        showFieldError(fieldName, issue.message)
      })
    }
  }
}
```

### React Example

```tsx
import { SchemaValidationError } from '@tanstack/db'

function TodoForm() {
  const [errors, setErrors] = useState<Record<string, string>>({})

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setErrors({})

    try {
      todoCollection.insert({
        id: crypto.randomUUID(),
        text: e.currentTarget.text.value,
        priority: parseInt(e.currentTarget.priority.value)
      })
    } catch (error) {
      if (error instanceof SchemaValidationError) {
        const newErrors: Record<string, string> = {}
        error.issues.forEach(issue => {
          const field = issue.path?.[0] || 'form'
          newErrors[field] = issue.message
        })
        setErrors(newErrors)
      }
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <input name="text" />
      {errors.text && <span className="error">{errors.text}</span>}

      <input name="priority" type="number" />
      {errors.priority && <span className="error">{errors.priority}</span>}

      <button type="submit">Add Todo</button>
    </form>
  )
}
```

---

## Best Practices

### Keep Transformations Simple

> **Performance Note:** Schema validation is synchronous and runs on every optimistic mutation. For high-frequency updates, keep transformations simple.

```typescript
// ❌ Avoid expensive operations
const schema = z.object({
  data: z.string().transform(val => {
    // Heavy computation on every mutation
    return expensiveParsingOperation(val)
  })
})

// ✅ Better: Validate only, process elsewhere
const schema = z.object({
  data: z.string()  // Simple validation
})

// Process in component or mutation handler when needed
const processedData = expensiveParsingOperation(todo.data)
```

### Use Union Types for Transformations (Essential)

When your schema transforms data to a different type, you **must** use union types to ensure TInput is a superset of TOutput. This is not optional - updates will fail without it.

```typescript
// ✅ REQUIRED: TInput accepts both string (new data) and Date (existing data)
const schema = z.object({
  created_at: z.union([z.string(), z.date()])
    .transform(val => typeof val === 'string' ? new Date(val) : val)
})
// TInput: { created_at: string | Date }
// TOutput: { created_at: Date }

// ❌ WILL BREAK: Updates fail because draft contains Date but TInput only accepts string
const schema = z.object({
  created_at: z.string().transform(val => new Date(val))
})
// TInput: { created_at: string }
// TOutput: { created_at: Date }
// Problem: collection.update() passes a Date to a schema expecting string!
```

**Why this is required:** During `collection.update()`, the `draft` object contains TOutput data (already transformed). The schema must accept this data, which means TInput must be a superset of TOutput.

### Validate at the Boundary

Let the collection schema handle validation. Don't duplicate validation logic:

```typescript
// ❌ Avoid: Duplicate validation
function addTodo(text: string) {
  if (!text || text.length < 3) {
    throw new Error("Text too short")
  }
  todoCollection.insert({ id: "1", text })
}

// ✅ Better: Let schema handle it
const todoSchema = z.object({
  id: z.string(),
  text: z.string().min(3, "Text must be at least 3 characters")
})
```

### Type Inference

Let TypeScript infer types from your schema:

```typescript
const todoSchema = z.object({
  id: z.string(),
  text: z.string(),
  completed: z.boolean()
})

type Todo = z.infer<typeof todoSchema>  // Inferred type

// ✅ Use the inferred type
const collection = createCollection(
  queryCollectionOptions({
    schema: todoSchema,
    // TypeScript knows the item type automatically
    getKey: (item) => item.id  // item is Todo
  })
)
```

### Custom Error Messages

Provide helpful error messages for users:

```typescript
const userSchema = z.object({
  username: z.string()
    .min(3, "Username must be at least 3 characters")
    .max(20, "Username is too long (max 20 characters)")
    .regex(/^[a-zA-Z0-9_]+$/, "Username can only contain letters, numbers, and underscores"),
  email: z.string().email("Please enter a valid email address"),
  age: z.number()
    .int("Age must be a whole number")
    .min(13, "You must be at least 13 years old")
})
```

---

## Full-Context Examples

### Example 1: Todo App with Rich Types

A complete todo application demonstrating validation, transformations, and defaults:

```typescript
import { z } from 'zod'
import { createCollection } from '@tanstack/react-db'
import { queryCollectionOptions } from '@tanstack/query-db-collection'

// Schema with validation, transformations, and defaults
const todoSchema = z.object({
  id: z.string(),
  text: z.string().min(1, "Todo text cannot be empty"),
  completed: z.boolean().default(false),
  priority: z.enum(['low', 'medium', 'high']).default('medium'),
  due_date: z.union([
    z.string(),
    z.date()
  ]).transform(val => typeof val === 'string' ? new Date(val) : val).optional(),
  created_at: z.union([
    z.string(),
    z.date()
  ]).transform(val => typeof val === 'string' ? new Date(val) : val)
    .default(() => new Date()),
  tags: z.array(z.string()).default([])
})

type Todo = z.infer<typeof todoSchema>

// Collection setup
const todoCollection = createCollection(
  queryCollectionOptions({
    queryKey: ['todos'],
    queryFn: async () => {
      const response = await fetch('/api/todos')
      const todos = await response.json()
      // Reuse schema to parse and transform API responses
      return todos.map((todo: any) => todoSchema.parse(todo))
    },
    getKey: (item) => item.id,
    schema: todoSchema,
    queryClient,

    onInsert: async ({ transaction }) => {
      const todo = transaction.mutations[0].modified

      // Serialize dates for API
      await fetch('/api/todos', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...todo,
          due_date: todo.due_date?.toISOString(),
          created_at: todo.created_at.toISOString()
        })
      })
    },

    onUpdate: async ({ transaction }) => {
      await Promise.all(
        transaction.mutations.map(async (mutation) => {
          const { original, changes } = mutation

          // Serialize any date fields in changes
          const serialized = {
            ...changes,
            due_date: changes.due_date instanceof Date
              ? changes.due_date.toISOString()
              : changes.due_date
          }

          await fetch(`/api/todos/${original.id}`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(serialized)
          })
        })
      )
    },

    onDelete: async ({ transaction }) => {
      await Promise.all(
        transaction.mutations.map(async (mutation) => {
          await fetch(`/api/todos/${mutation.original.id}`, {
            method: 'DELETE'
          })
        })
      )
    }
  })
)

// Component usage
function TodoApp() {
  const { data: todos } = useLiveQuery(q =>
    q.from({ todo: todoCollection })
      .where(({ todo }) => !todo.completed)
      .orderBy(({ todo }) => todo.created_at, 'desc')
  )

  const [errors, setErrors] = useState<Record<string, string>>({})

  const addTodo = (text: string, priority: 'low' | 'medium' | 'high') => {
    try {
      todoCollection.insert({
        id: crypto.randomUUID(),
        text,
        priority,
        due_date: "2024-12-31T23:59:59Z"
        // completed, created_at, tags filled automatically by defaults
      })
      setErrors({})
    } catch (error) {
      if (error instanceof SchemaValidationError) {
        const newErrors: Record<string, string> = {}
        error.issues.forEach(issue => {
          const field = issue.path?.[0] || 'form'
          newErrors[field] = issue.message
        })
        setErrors(newErrors)
      }
    }
  }

  const toggleComplete = (todo: Todo) => {
    todoCollection.update(todo.id, (draft) => {
      draft.completed = !draft.completed
    })
  }

  return (
    <div>
      <h1>Todos</h1>

      {errors.text && <div className="error">{errors.text}</div>}

      <button onClick={() => addTodo("Buy groceries", "high")}>
        Add Todo
      </button>

      <ul>
        {todos?.map(todo => (
          <li key={todo.id}>
            <input
              type="checkbox"
              checked={todo.completed}
              onChange={() => toggleComplete(todo)}
            />
            <span>{todo.text}</span>
            <span>Priority: {todo.priority}</span>
            {todo.due_date && (
              <span>Due: {todo.due_date.toLocaleDateString()}</span>
            )}
            <span>Created: {todo.created_at.toLocaleDateString()}</span>
          </li>
        ))}
      </ul>
    </div>
  )
}
```

### Example 2: E-commerce Product with Computed Fields

```typescript
import { z } from 'zod'

// Schema with computed fields and transformations
const productSchema = z.object({
  id: z.string(),
  name: z.string().min(3, "Product name must be at least 3 characters"),
  description: z.string().max(500, "Description too long"),
  base_price: z.number().positive("Price must be positive"),
  tax_rate: z.number().min(0).max(1).default(0.1),
  discount_percent: z.number().min(0).max(100).default(0),
  stock: z.number().int().min(0).default(0),
  category: z.enum(['electronics', 'clothing', 'food', 'other']),
  tags: z.array(z.string()).default([]),
  created_at: z.union([z.string(), z.date()])
    .transform(val => typeof val === 'string' ? new Date(val) : val)
    .default(() => new Date())
}).transform(data => ({
  ...data,
  // Computed fields
  final_price: data.base_price * (1 + data.tax_rate) * (1 - data.discount_percent / 100),
  in_stock: data.stock > 0,
  display_price: `$${(data.base_price * (1 + data.tax_rate) * (1 - data.discount_percent / 100)).toFixed(2)}`
}))

type Product = z.infer<typeof productSchema>

const productCollection = createCollection(
  queryCollectionOptions({
    queryKey: ['products'],
    queryFn: async () => api.products.getAll(),
    getKey: (item) => item.id,
    schema: productSchema,
    queryClient,

    onInsert: async ({ transaction }) => {
      const product = transaction.mutations[0].modified

      // API only needs base fields, not computed ones
      await api.products.create({
        name: product.name,
        description: product.description,
        base_price: product.base_price,
        tax_rate: product.tax_rate,
        discount_percent: product.discount_percent,
        stock: product.stock,
        category: product.category,
        tags: product.tags
      })
    }
  })
)

// Usage
function ProductList() {
  const { data: products } = useLiveQuery(q =>
    q.from({ product: productCollection })
      .where(({ product }) => product.in_stock)  // Use computed field
      .orderBy(({ product }) => product.final_price, 'asc')
  )

  const addProduct = () => {
    productCollection.insert({
      id: crypto.randomUUID(),
      name: "Wireless Mouse",
      description: "Ergonomic wireless mouse",
      base_price: 29.99,
      discount_percent: 10,
      category: "electronics",
      stock: 50
      // tax_rate, tags, created_at filled by defaults
      // final_price, in_stock, display_price computed automatically
    })
  }

  return (
    <div>
      {products?.map(product => (
        <div key={product.id}>
          <h3>{product.name}</h3>
          <p>{product.description}</p>
          <p>Price: {product.display_price}</p>
          <p>Stock: {product.in_stock ? `${product.stock} available` : 'Out of stock'}</p>
          <p>Category: {product.category}</p>
        </div>
      ))}
    </div>
  )
}
```

---

## For Integration Authors

If you're building a custom collection (like Electric or TrailBase), you'll need to handle data parsing and serialization between your storage format and the in-memory collection format. This is separate from schema validation, which happens during client mutations.

See the [Collection Options Creator Guide](./collection-options-creator.md) for comprehensive documentation on creating custom collection integrations, including how to handle schemas, data parsing, and type transformations.

---

## Related Topics

- **[Mutations Guide](./mutations.md)** - Learn about optimistic mutations and how schemas validate mutation data
- **[Error Handling Guide](./error-handling.md)** - Comprehensive guide to handling `SchemaValidationError` and other errors
- **[Collection Options Creator Guide](./collection-options-creator.md)** - For integration authors: creating custom collection types with schema support
- **[StandardSchema Specification](https://standardschema.dev)** - Full specification for StandardSchema v1


---


---
title: Error Handling
id: error-handling
---

# Error Handling

TanStack DB provides comprehensive error handling capabilities to ensure robust data synchronization and state management. This guide covers the built-in error handling mechanisms and how to work with them effectively.

## Error Types

TanStack DB provides named error classes for better error handling and type safety. All error classes can be imported from `@tanstack/db` (or more commonly, the framework-specific package e.g. `@tanstack/react-db`):

```ts
import {
  SchemaValidationError,
  CollectionInErrorStateError,
  DuplicateKeyError,
  MissingHandlerError,
  TransactionError,
  // ... and many more
} from "@tanstack/db"
```

### SchemaValidationError

Thrown when data doesn't match the collection's schema during insert or update operations:

```ts
import { SchemaValidationError } from "@tanstack/db"

try {
  todoCollection.insert({ text: 123 }) // Invalid type
} catch (error) {
  if (error instanceof SchemaValidationError) {
    console.log(error.type) // 'insert' or 'update'
    console.log(error.issues) // Array of validation issues
    // Example issue: { message: "Expected string, received number", path: ["text"] }
  }
}
```

The error includes:
- `type`: Whether it was an 'insert' or 'update' operation
- `issues`: Array of validation issues with messages and paths
- `message`: A formatted error message listing all issues

**When schema validation occurs:**

Schema validation happens only for **client mutations** - when you explicitly insert or update data:

1. **During inserts** - When `collection.insert()` is called
2. **During updates** - When `collection.update()` is called

Schemas do **not** validate data coming from your server or sync layer. That data is assumed to already be valid.

```typescript
const schema = z.object({
  id: z.string(),
  created_at: z.string().transform(val => new Date(val))
  // TInput: string, TOutput: Date
})

// Validation happens here ✓
collection.insert({
  id: "1",
  created_at: "2024-01-01"  // TInput: string
})
// If successful, stores: { created_at: Date }  // TOutput: Date
```

For more details on schema validation and type transformations, see the [Schemas guide](./schemas.md).

## Query Collection Error Tracking

Query collections provide enhanced error tracking utilities through the `utils` object. These methods expose error state information and provide recovery mechanisms for failed queries:

```tsx
import { createCollection } from "@tanstack/db"
import { queryCollectionOptions } from "@tanstack/query-db-collection"
import { useLiveQuery } from "@tanstack/react-db"

const syncedCollection = createCollection(
  queryCollectionOptions({
    queryClient,
    queryKey: ['synced-data'],
    queryFn: fetchData,
    getKey: (item) => item.id,
  })
)

// Component can check error state
function DataList() {
  const { data } = useLiveQuery((q) => q.from({ item: syncedCollection }))
  const isError = syncedCollection.utils.isError
  const errorCount = syncedCollection.utils.errorCount
  
  return (
    <>
      {isError && errorCount > 3 && (
        <Alert>
          Unable to sync. Showing cached data.
          <button onClick={() => syncedCollection.utils.clearError()}>
            Retry
          </button>
        </Alert>
      )}
      {/* Render data */}
    </>
  )
}
```

Error tracking methods:
- **`lastError`**: Returns the most recent error encountered by the query, or `undefined` if no errors have occurred:
- **`isError`**: Returns a boolean indicating whether the collection is currently in an error state:
- **`errorCount`**: Returns the number of consecutive sync failures. This counter is incremented only when queries fail completely (not per retry attempt) and is reset on successful queries:
- **`clearError()`**: Clears the error state and triggers a refetch of the query. This method resets both `lastError` and `errorCount`:

## Collection Status and Error States

Collections track their status and transition between states:

```tsx
import { useLiveQuery } from "@tanstack/react-db"

const TodoList = () => {
  const { data, status, isError, isLoading, isReady } = useLiveQuery(
    (query) => query.from({ todos: todoCollection })
  )

  if (isError) {
    return <div>Collection is in error state</div>
  }

  if (isLoading) {
    return <div>Loading...</div>
  }

  return <div>{data?.map(todo => <div key={todo.id}>{todo.text}</div>)}</div>
}
```

Collection status values:
- `idle` - Not yet started
- `loading` - Loading initial data
- `initialCommit` - Processing initial data
- `ready` - Ready for use
- `error` - In error state
- `cleaned-up` - Cleaned up and no longer usable

### Using Suspense and Error Boundaries (React)

For React applications, you can handle loading and error states with `useLiveSuspenseQuery`, React Suspense, and Error Boundaries:

```tsx
import { useLiveSuspenseQuery } from "@tanstack/react-db"
import { Suspense } from "react"
import { ErrorBoundary } from "react-error-boundary"

const TodoList = () => {
  // No need to check status - Suspense and ErrorBoundary handle it
  const { data } = useLiveSuspenseQuery(
    (query) => query.from({ todos: todoCollection })
  )

  // data is always defined here
  return <div>{data.map(todo => <div key={todo.id}>{todo.text}</div>)}</div>
}

const App = () => (
  <ErrorBoundary fallback={<div>Failed to load todos</div>}>
    <Suspense fallback={<div>Loading...</div>}>
      <TodoList />
    </Suspense>
  </ErrorBoundary>
)
```

With this approach, loading states are handled by `<Suspense>` and error states are handled by `<ErrorBoundary>` instead of within your component logic. See the [React Suspense section in Live Queries](./live-queries#using-with-react-suspense) for more details.

## Transaction Error Handling

When mutations fail, TanStack DB automatically rolls back optimistic updates:

```ts
const todoCollection = createCollection({
  id: "todos",
  onInsert: async ({ transaction }) => {
    const response = await fetch("/api/todos", {
      method: "POST",
      body: JSON.stringify(transaction.mutations[0].modified),
    })
    
    if (!response.ok) {
      // Throwing an error will rollback the optimistic state
      throw new Error(`HTTP Error: ${response.status}`)
    }
    
    return response.json()
  },
})

// Usage - optimistic update will be rolled back if the mutation fails
try {
  const tx = todoCollection.insert({
    id: "1",
    text: "New todo",
    completed: false,
  })
  
  await tx.isPersisted.promise
} catch (error) {
  // The optimistic update has been automatically rolled back
  console.error("Failed to create todo:", error)
}
```

### Transaction States and Error Information

Transactions have the following states:
- `pending` - Transaction is being processed
- `persisting` - Currently executing the mutation function
- `completed` - Transaction completed successfully
- `failed` - Transaction failed and was rolled back

Access transaction error information from collection operations:

```ts
const todoCollection = createCollection({
  id: "todos",
  onUpdate: async ({ transaction }) => {
    const response = await fetch(`/api/todos/${transaction.mutations[0].key}`, {
      method: "PUT",
      body: JSON.stringify(transaction.mutations[0].modified),
    })
    
    if (!response.ok) {
      throw new Error(`Update failed: ${response.status}`)
    }
  },
})

try {
  const tx = await todoCollection.update("todo-1", (draft) => {
    draft.completed = true
  })
  
  await tx.isPersisted.promise
} catch (error) {
  // Transaction has been rolled back
  console.log(tx.state) // "failed"
  console.log(tx.error) // { message: "Update failed: 500", error: Error }
}
```

Or with manual transaction creation:

```ts
const tx = createTransaction({
  mutationFn: async ({ transaction }) => {
    throw new Error("API failed")
  }
})

tx.mutate(() => {
  collection.insert({ id: "1", text: "Item" })
})

try {
  await tx.commit()
} catch (error) {
  // Transaction has been rolled back
  console.log(tx.state) // "failed"
  console.log(tx.error) // { message: "API failed", error: Error }
}
```

## Collection Operation Errors

### Invalid Collection State

Collections in an `error` state cannot perform operations and must be manually recovered:

```ts
import { CollectionInErrorStateError } from "@tanstack/db"

try {
  todoCollection.insert(newTodo)
} catch (error) {
  if (error instanceof CollectionInErrorStateError) {
    // Collection needs to be cleaned up and restarted
    await todoCollection.cleanup()
    
    // Now retry the operation
    todoCollection.insert(newTodo)
  }
}
```

### Missing Mutation Handlers

Direct mutations require handlers to be configured:

```ts
const todoCollection = createCollection({
  id: "todos",
  getKey: (todo) => todo.id,
  // Missing onInsert handler
})

// This will throw an error
todoCollection.insert(newTodo)
// Error: Collection.insert called directly (not within an explicit transaction) but no 'onInsert' handler is configured
```

### Insert Operation Errors

#### DuplicateKeyError

Thrown when inserting items with existing keys:

```ts
import { DuplicateKeyError } from "@tanstack/db"

try {
  todoCollection.insert({ id: "existing-id", text: "Todo" })
} catch (error) {
  if (error instanceof DuplicateKeyError) {
    console.log(`Duplicate key: ${error.message}`)
    // Consider using update() instead, or check if item exists first
  }
}
```

#### UndefinedKeyError

Thrown when an object is created without a defined key:

```ts
import { UndefinedKeyError } from "@tanstack/db"

const collection = createCollection({
  id: "todos",
  getKey: (item) => item.id,
})

try {
  collection.insert({ text: "Todo" }) // Missing 'id' field
} catch (error) {
  if (error instanceof UndefinedKeyError) {
    console.log("Item is missing required key field")
    // Ensure your items have the key field defined by getKey
  }
}
```

### Update Operation Errors

#### UpdateKeyNotFoundError

Thrown when trying to update a key that doesn't exist in the collection:

```ts
import { UpdateKeyNotFoundError } from "@tanstack/db"

try {
  todoCollection.update("nonexistent-key", draft => {
    draft.completed = true
  })
} catch (error) {
  if (error instanceof UpdateKeyNotFoundError) {
    console.log("Key not found - item may have been deleted")
    // Consider using insert() if the item doesn't exist
  }
}
```

#### KeyUpdateNotAllowedError

Thrown when attempting to change an item's key (not allowed - delete and re-insert instead):

```ts
import { KeyUpdateNotAllowedError } from "@tanstack/db"

try {
  todoCollection.update("todo-1", draft => {
    draft.id = "todo-2" // Not allowed!
  })
} catch (error) {
  if (error instanceof KeyUpdateNotAllowedError) {
    console.log("Cannot change item keys")
    // Instead, delete the old item and insert a new one
  }
}
```

### Delete Operation Errors

#### DeleteKeyNotFoundError

Thrown when trying to delete a key that doesn't exist:

```ts
import { DeleteKeyNotFoundError } from "@tanstack/db"

try {
  todoCollection.delete("nonexistent-key")
} catch (error) {
  if (error instanceof DeleteKeyNotFoundError) {
    console.log("Key not found - item may have already been deleted")
    // This may be acceptable in some scenarios (idempotent deletes)
  }
}
```

## Sync Error Handling

### Query Collection Sync Errors

Query collections handle sync errors gracefully and mark the collection as ready even on error to avoid blocking applications:

```ts
import { queryCollectionOptions } from "@tanstack/query-db-collection"

const todoCollection = createCollection(
  queryCollectionOptions({
    queryKey: ["todos"],
    queryFn: async () => {
      const response = await fetch("/api/todos")
      if (!response.ok) {
        throw new Error(`Failed to fetch: ${response.status}`)
      }
      return response.json()
    },
    queryClient,
    getKey: (item) => item.id,
    schema: todoSchema,
    // Standard TanStack Query error handling options
    retry: 3,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  })
)
```

When sync errors occur:
- Error is logged to console: `[QueryCollection] Error observing query...`
- Collection is marked as ready to prevent blocking the application
- Cached data remains available
- Error tracking counters are updated (`lastError`, `errorCount`)

### Sync Write Errors

Sync functions must handle their own errors during write operations:

```ts
const collection = createCollection({
  id: "todos",
  sync: {
    sync: ({ begin, write, commit }) => {
      begin()
      
      try {
        // Will throw if key already exists
        write({ type: "insert", value: { id: "existing-id", text: "Todo" } })
      } catch (error) {
        // Error: Cannot insert document with key "existing-id" from sync because it already exists
      }
      
      commit()
    }
  }
})
```

### Cleanup Error Handling

Cleanup errors are isolated to prevent blocking the cleanup process:

```ts
const collection = createCollection({
  id: "todos",
  sync: {
    sync: ({ begin, commit }) => {
      begin()
      commit()
      
      // Return a cleanup function
      return () => {
        // If this throws, the error is re-thrown in a microtask
        // but cleanup continues successfully
        throw new Error("Sync cleanup failed")
      }
    },
  },
})

// Cleanup completes even if the sync cleanup function throws
await collection.cleanup() // Resolves successfully
// Error is re-thrown asynchronously via queueMicrotask
```

## Error Recovery Patterns

### Collection Cleanup and Restart

Clean up collections in error states:

```ts
if (todoCollection.status === "error") {
  // Cleanup will stop sync and reset the collection
  await todoCollection.cleanup()
  
  // Collection will automatically restart on next access
  todoCollection.preload() // Or any other operation
}
```

### Graceful Degradation

Collections continue to work with cached data even when sync fails:

```tsx
const TodoApp = () => {
  const { data, isError } = useLiveQuery((query) => 
    query.from({ todos: todoCollection })
  )

  return (
    <div>
      {isError && (
        <div>Sync failed, but you can still view cached data</div>
      )}
      {data?.map(todo => <TodoItem key={todo.id} todo={todo} />)}
    </div>
  )
}
```

### Transaction Rollback Cascading

When a transaction fails, conflicting transactions are automatically rolled back:

```ts
const tx1 = createTransaction({ mutationFn: async () => {} })
const tx2 = createTransaction({ mutationFn: async () => {} })

tx1.mutate(() => collection.update("1", draft => { draft.value = "A" }))
tx2.mutate(() => collection.update("1", draft => { draft.value = "B" })) // Same item

// Rolling back tx1 will also rollback tx2 due to conflict
tx1.rollback() // tx2 is automatically rolled back
```

### Transaction Lifecycle Errors

Transactions validate their state before operations to prevent misuse. Here are the specific errors you may encounter:

#### MissingMutationFunctionError

Thrown when creating a transaction without a required `mutationFn`:

```ts
import { MissingMutationFunctionError } from "@tanstack/db"

try {
  const tx = createTransaction({}) // Missing mutationFn
} catch (error) {
  if (error instanceof MissingMutationFunctionError) {
    console.log("mutationFn is required when creating a transaction")
  }
}
```

#### TransactionNotPendingMutateError

Thrown when calling `mutate()` after a transaction is no longer pending:

```ts
import { TransactionNotPendingMutateError } from "@tanstack/db"

const tx = createTransaction({ mutationFn: async () => {} })

await tx.commit()

try {
  tx.mutate(() => {
    collection.insert({ id: "1", text: "Item" })
  })
} catch (error) {
  if (error instanceof TransactionNotPendingMutateError) {
    console.log("Cannot mutate - transaction is no longer pending")
  }
}
```

#### TransactionNotPendingCommitError

Thrown when calling `commit()` after a transaction is no longer pending:

```ts
import { TransactionNotPendingCommitError } from "@tanstack/db"

const tx = createTransaction({ mutationFn: async () => {} })
tx.mutate(() => collection.insert({ id: "1", text: "Item" }))

await tx.commit()

try {
  await tx.commit() // Trying to commit again
} catch (error) {
  if (error instanceof TransactionNotPendingCommitError) {
    console.log("Transaction already committed")
  }
}
```

#### TransactionAlreadyCompletedRollbackError

Thrown when calling `rollback()` on a transaction that's already completed:

```ts
import { TransactionAlreadyCompletedRollbackError } from "@tanstack/db"

const tx = createTransaction({ mutationFn: async () => {} })
tx.mutate(() => collection.insert({ id: "1", text: "Item" }))

await tx.commit()

try {
  tx.rollback() // Can't rollback after commit
} catch (error) {
  if (error instanceof TransactionAlreadyCompletedRollbackError) {
    console.log("Cannot rollback - transaction already completed")
  }
}
```

### Sync Transaction Errors

When working with sync transactions, these errors can occur:

#### NoPendingSyncTransactionWriteError

Thrown when calling `write()` without an active sync transaction:

```ts
const collection = createCollection({
  id: "todos",
  sync: {
    sync: ({ write }) => {
      // Calling write without begin() first
      write({ type: "insert", value: { id: "1", text: "Todo" } })
      // Error: No pending sync transaction to write to
    }
  }
})
```

#### SyncTransactionAlreadyCommittedWriteError

Thrown when calling `write()` after the sync transaction is already committed:

```ts
const collection = createCollection({
  id: "todos",
  sync: {
    sync: ({ begin, write, commit }) => {
      begin()
      commit()

      // Trying to write after commit
      write({ type: "insert", value: { id: "1", text: "Todo" } })
      // Error: The pending sync transaction is already committed
    }
  }
})
```

#### NoPendingSyncTransactionCommitError

Thrown when calling `commit()` without an active sync transaction.

#### SyncTransactionAlreadyCommittedError

Thrown when calling `commit()` on a sync transaction that's already committed.

## Best Practices

1. **Use instanceof checks** - Use `instanceof` instead of string matching for error handling:
   ```ts
   // ✅ Good - type-safe error handling
   if (error instanceof SchemaValidationError) {
     // Handle validation error
   }
   
   // ❌ Avoid - brittle string matching  
   if (error.message.includes("validation failed")) {
     // Handle validation error
   }
   ```

2. **Import specific error types** - Import only the error classes you need for better tree-shaking
3. **Always handle SchemaValidationError** - Provide clear feedback for validation failures
4. **Check collection status** - Use `isError`, `isLoading`, `isReady` flags in React components
5. **Handle transaction promises** - Always handle `isPersisted.promise` rejections

## Example: Complete Error Handling

```tsx
import {
  createCollection,
  SchemaValidationError,
  DuplicateKeyError,
  UpdateKeyNotFoundError,
  DeleteKeyNotFoundError,
  TransactionNotPendingCommitError,
  createTransaction
} from "@tanstack/db"
import { useLiveQuery } from "@tanstack/react-db"

const todoCollection = createCollection({
  id: "todos",
  schema: todoSchema,
  getKey: (todo) => todo.id,
  onInsert: async ({ transaction }) => {
    const response = await fetch("/api/todos", {
      method: "POST",
      body: JSON.stringify(transaction.mutations[0].modified),
    })

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }

    return response.json()
  },
  sync: {
    sync: ({ begin, write, commit }) => {
      // Your sync implementation
      begin()
      // ... sync logic
      commit()
    }
  }
})

const TodoApp = () => {
  const { data, status, isError, isLoading } = useLiveQuery(
    (query) => query.from({ todos: todoCollection })
  )

  const handleAddTodo = async (text: string) => {
    try {
      const tx = await todoCollection.insert({
        id: crypto.randomUUID(),
        text,
        completed: false,
      })
      
      // Wait for persistence
      await tx.isPersisted.promise
    } catch (error) {
      if (error instanceof SchemaValidationError) {
        alert(`Validation error: ${error.issues[0]?.message}`)
      } else if (error instanceof DuplicateKeyError) {
        alert("A todo with this ID already exists")
      } else {
        alert(`Failed to add todo: ${error.message}`)
      }
    }
  }

  const handleCleanup = async () => {
    try {
      await todoCollection.cleanup()
      // Collection will restart on next access
    } catch (error) {
      console.error("Cleanup failed:", error)
    }
  }

  if (isError) {
    return (
      <div>
        <div>Collection error - data may be stale</div>
        <button onClick={handleCleanup}>
          Restart Collection
        </button>
      </div>
    )
  }

  if (isLoading) {
    return <div>Loading todos...</div>
  }

  return (
    <div>
      <button onClick={() => handleAddTodo("New todo")}>
        Add Todo
      </button>
      {data?.map(todo => (
        <div key={todo.id}>{todo.text}</div>
      ))}
    </div>
  )
}
```

## See Also

- [API Reference](../overview.md#api-reference) - Detailed API documentation
- [Mutations Guide](../overview.md#making-optimistic-mutations) - Learn about optimistic updates and rollbacks
- [TanStack Query Error Handling](https://tanstack.com/query/latest/docs/react/guides/error-handling) - Query-specific error handling


---


---
title: Creating a Collection Options Creator
id: guide/collection-options-creator
---
# Creating a Collection Options Creator

A collection options creator is a factory function that generates configuration options for TanStack DB collections. It provides a standardized way to integrate different sync engines and data sources with TanStack DB's reactive sync-first architecture.

## Overview

Collection options creators follow a consistent pattern:
1. Accept configuration specific to the sync engine
2. Return an object that satisfies the `CollectionConfig` interface
3. Handle sync initialization, data parsing, and transaction management
4. Optionally provide utility functions specific to the sync engine

## When to Create a Custom Collection

You should create a custom collection when:
- You have a dedicated sync engine (like ElectricSQL, Trailbase, Firebase, RxDB or a custom WebSocket solution)
- You need specific sync behaviors that aren't covered by the query collection
- You want to integrate with a backend that has its own sync protocol

**Note**: If you're just hitting an API and returning data, use the query collection instead.

## Core Requirements

Every collection options creator must implement these key responsibilities:

### 1. Configuration Interface

Define a configuration interface that extends or includes standard collection properties:

```typescript
// Pattern A: User provides handlers (Query / ElectricSQL style)
interface MyCollectionConfig<TItem extends object> {
  // Your sync engine specific options
  connectionUrl: string
  apiKey?: string
  
  // Standard collection properties
  id?: string
  schema?: StandardSchemaV1
  getKey: (item: TItem) => string | number
  sync?: SyncConfig<TItem>
  
  rowUpdateMode?: 'partial' | 'full'
  
  // User provides mutation handlers
  onInsert?: InsertMutationFn<TItem>
  onUpdate?: UpdateMutationFn<TItem>
  onDelete?: DeleteMutationFn<TItem>
}

// Pattern B: Built-in handlers (Trailbase style)
interface MyCollectionConfig<TItem extends object> 
  extends Omit<CollectionConfig<TItem>, 'onInsert' | 'onUpdate' | 'onDelete'> {
  // Your sync engine specific options
  recordApi: MyRecordApi<TItem>
  connectionUrl: string
  
  rowUpdateMode?: 'partial' | 'full'
  
  // Note: onInsert/onUpdate/onDelete are implemented by your collection creator
}
```

### 2. Sync Implementation

The sync function is the heart of your collection. It must:

The sync function must return a cleanup function for proper garbage collection:

```typescript
const sync: SyncConfig<T>['sync'] = (params) => {
  const { begin, write, commit, markReady, collection } = params
  
  // 1. Initialize connection to your sync engine
  const connection = initializeConnection(config)
  
  // 2. Set up real-time subscription FIRST (prevents race conditions)
  const eventBuffer: Array<any> = []
  let isInitialSyncComplete = false
  
  connection.subscribe((event) => {
    if (!isInitialSyncComplete) {
      // Buffer events during initial sync to prevent race conditions
      eventBuffer.push(event)
      return
    }
    
    // Process real-time events
    begin()
    
    switch (event.type) {
      case 'insert':
        write({ type: 'insert', value: event.data })
        break
      case 'update':
        write({ type: 'update', value: event.data })
        break
      case 'delete':
        write({ type: 'delete', value: event.data })
        break
    }
    
    commit()
  })
  
  // 3. Perform initial data fetch
  async function initialSync() {
    try {
      const data = await fetchInitialData()
      
      begin() // Start a transaction
      
      for (const item of data) {
        write({
          type: 'insert',
          value: item
        })
      }
      
      commit() // Commit the transaction
      
      // 4. Process buffered events
      isInitialSyncComplete = true
      if (eventBuffer.length > 0) {
        begin()
        for (const event of eventBuffer) {
          // Deduplicate if necessary based on your sync engine
          write({ type: event.type, value: event.data })
        }
        commit()
        eventBuffer.splice(0)
      }
      
    } catch (error) {
      console.error('Initial sync failed:', error)
      throw error
    } finally {
      // ALWAYS call markReady, even on error
      markReady()
    }
  }

  initialSync()
  
  // 4. Return cleanup function
  return () => {
    connection.close()
    // Clean up any timers, intervals, or other resources
  }
}
```

### 3. Transaction Lifecycle

Understanding the transaction lifecycle is important for correct implementation.

The sync process follows this lifecycle:

1. **begin()** - Start collecting changes
2. **write()** - Add changes to the pending transaction (buffered until commit)
3. **commit()** - Apply all changes atomically to the collection state
4. **markReady()** - Signal that initial sync is complete

**Race Condition Prevention:**
Many sync engines start real-time subscriptions before the initial sync completes. Your implementation MUST deduplicate events that arrive via subscription that represent the same data as the initial sync. Consider:
- Starting the listener BEFORE initial fetch and buffering events
- Tracking timestamps, sequence numbers, or document versions
- Using read timestamps or other ordering mechanisms

### 4. Data Parsing and Type Conversion

If your sync engine returns data with different types, provide conversion functions for specific fields:

```typescript
interface MyCollectionConfig<TItem, TRecord> {
  // ... other config
  
  // Only specify conversions for fields that need type conversion
  parse: {
    created_at: (ts: number) => new Date(ts * 1000),  // timestamp -> Date
    updated_at: (ts: number) => new Date(ts * 1000),  // timestamp -> Date
    metadata?: (str: string) => JSON.parse(str)       // JSON string -> object
  }
  
  serialize: {
    created_at: (date: Date) => Math.floor(date.valueOf() / 1000),  // Date -> timestamp
    updated_at: (date: Date) => Math.floor(date.valueOf() / 1000),  // Date -> timestamp  
    metadata?: (obj: object) => JSON.stringify(obj)                 // object -> JSON string
  }
}
```

**Type Conversion Examples:**
```typescript
// Firebase Timestamp to Date
parse: {
  createdAt: (timestamp) => timestamp?.toDate?.() || new Date(timestamp),
  updatedAt: (timestamp) => timestamp?.toDate?.() || new Date(timestamp),
}

// PostGIS geometry to GeoJSON
parse: {
  location: (wkb: string) => parseWKBToGeoJSON(wkb)
}

// JSON string to object with error handling
parse: {
  metadata: (str: string) => {
    try {
      return JSON.parse(str)
    } catch {
      return {}
    }
  }
}
```

### 5. Schemas and Type Transformations

When building a custom collection, you need to decide how to handle the relationship between your backend's storage format and the client-side types users work with in their collections.

#### Two Separate Concerns

**Backend Format** - The types your storage layer uses (SQLite, Postgres, Firebase, etc.)
- Examples: Unix timestamps, ISO strings, JSON strings, PostGIS geometries

**Client Format** - The types users work with in their TanStack DB collections
- Examples: Date objects, parsed JSON, GeoJSON

Schemas in TanStack DB define the **client format** (TInput/TOutput for mutations). How you bridge between backend and client format depends on your integration design.

#### Approach 1: Integration Provides Parse/Serialize Helpers

For backends with specific storage formats, provide `parse`/`serialize` options that users configure:

```typescript
// TrailBase example: User specifies field conversions
export function trailbaseCollectionOptions(config) {
  return {
    parse: config.parse,      // User provides field conversions
    serialize: config.serialize,

    onInsert: async ({ transaction }) => {
      const serialized = transaction.mutations.map(m =>
        serializeFields(m.modified, config.serialize)
      )
      await config.recordApi.createBulk(serialized)
    }
  }
}

// User explicitly configures conversions
const collection = createCollection(
  trailbaseCollectionOptions({
    schema: todoSchema,
    parse: {
      created_at: (ts: number) => new Date(ts * 1000)  // Unix → Date
    },
    serialize: {
      created_at: (date: Date) => Math.floor(date.valueOf() / 1000)  // Date → Unix
    }
  })
)
```

**Benefits:** Explicit control over type conversions. Integration handles applying them consistently.

#### Approach 2: User Handles Everything in QueryFn/Handlers

For simple APIs or when users want full control, they handle parsing/serialization themselves:

```typescript
// Query Collection: User handles all transformations
const collection = createCollection(
  queryCollectionOptions({
    schema: todoSchema,
    queryFn: async () => {
      const response = await fetch('/api/todos')
      const todos = await response.json()
      // User manually parses to match their schema's TOutput
      return todos.map(todo => ({
        ...todo,
        created_at: new Date(todo.created_at)  // ISO string → Date
      }))
    },
    onInsert: async ({ transaction }) => {
      // User manually serializes for their backend
      await fetch('/api/todos', {
        method: 'POST',
        body: JSON.stringify({
          ...transaction.mutations[0].modified,
          created_at: transaction.mutations[0].modified.created_at.toISOString()  // Date → ISO string
        })
      })
    }
  })
)
```

**Benefits:** Maximum flexibility, no abstraction overhead. Users see exactly what's happening.

#### Approach 3: Automatic Serialization in Handlers

If your backend has well-defined types, you can automatically serialize in mutation handlers:

```typescript
export function myCollectionOptions(config) {
  return {
    onInsert: async ({ transaction }) => {
      // Automatically serialize known types for your backend
      const serialized = transaction.mutations.map(m => ({
        ...m.modified,
        // Date objects → Unix timestamps for your backend
        created_at: m.modified.created_at instanceof Date
          ? Math.floor(m.modified.created_at.valueOf() / 1000)
          : m.modified.created_at
      }))
      await backend.insert(serialized)
    }
  }
}
```

**Benefits:** Least configuration for users. Integration handles backend format automatically.

#### Key Design Principles

1. **Schemas validate client mutations only** - They don't affect how backend data is parsed during sync
2. **TOutput is the application-facing type** - This is what users work with in their app
3. **Choose your approach based on backend constraints** - Fixed types → automatic serialization; varying types → user configuration
4. **Document your backend format clearly** - Explain what types your storage uses and how to handle them

For more on schemas from a user perspective, see the [Schemas guide](./schemas.md).

### 6. Mutation Handler Patterns

There are two distinct patterns for handling mutations in collection options creators:

#### Pattern A: User-Provided Handlers (ElectricSQL, Query)

The user provides mutation handlers in the config. Your collection creator passes them through:

```typescript
interface MyCollectionConfig<TItem extends object> {
  // ... other config
  
  // User provides these handlers
  onInsert?: InsertMutationFn<TItem>
  onUpdate?: UpdateMutationFn<TItem>
  onDelete?: DeleteMutationFn<TItem>
}

export function myCollectionOptions<TItem extends object>(
  config: MyCollectionConfig<TItem>
) {
  return {
    // ... other options
    rowUpdateMode: config.rowUpdateMode || 'partial',
    
    // Pass through user-provided handlers (possibly with additional logic)
    onInsert: config.onInsert ? async (params) => {
      const result = await config.onInsert!(params)
      // Additional sync coordination logic
      return result
    } : undefined
  }
}
```

#### Pattern B: Built-in Handlers (Trailbase, WebSocket, Firebase)

Your collection creator implements the handlers directly using the sync engine's APIs:

```typescript
interface MyCollectionConfig<TItem extends object> 
  extends Omit<CollectionConfig<TItem>, 'onInsert' | 'onUpdate' | 'onDelete'> {
  // ... sync engine specific config
  // Note: onInsert/onUpdate/onDelete are NOT in the config
}

export function myCollectionOptions<TItem extends object>(
  config: MyCollectionConfig<TItem>
) {
  return {
    // ... other options
    rowUpdateMode: config.rowUpdateMode || 'partial',
    
    // Implement handlers using sync engine APIs
    onInsert: async ({ transaction }) => {
      // Handle provider-specific batch limits (e.g., Firestore's 500 limit)
      const chunks = chunkArray(transaction.mutations, PROVIDER_BATCH_LIMIT)
      
      for (const chunk of chunks) {
        const ids = await config.recordApi.createBulk(
          chunk.map(m => serialize(m.modified))
        )
        await awaitIds(ids)
      }
      
      return transaction.mutations.map(m => m.key)
    },
    
    onUpdate: async ({ transaction }) => {
      const chunks = chunkArray(transaction.mutations, PROVIDER_BATCH_LIMIT)
      
      for (const chunk of chunks) {
        await Promise.all(
          chunk.map(m => 
            config.recordApi.update(m.key, serialize(m.changes))
          )
        )
      }
      
      await awaitIds(transaction.mutations.map(m => String(m.key)))
    }
  }
}
```

Many providers have batch size limits (Firestore: 500, DynamoDB: 25, etc.) so chunk large transactions accordingly.

Choose Pattern A when users need to provide their own APIs, and Pattern B when your sync engine handles writes directly.

## Row Update Modes

Collections support two update modes:

- **`partial`** (default) - Updates are merged with existing data
- **`full`** - Updates replace the entire row

Configure this in your sync config:

```typescript
sync: {
  sync: syncFn,
  rowUpdateMode: 'full' // or 'partial'
}
```

## Production Examples

For complete, production-ready examples, see the collection packages in the TanStack DB repository:

- **[@tanstack/query-collection](https://github.com/TanStack/db/tree/main/packages/query-collection)** - Pattern A: User-provided handlers with full refetch strategy
- **[@tanstack/trailbase-collection](https://github.com/TanStack/db/tree/main/packages/trailbase-collection)** - Pattern B: Built-in handlers with ID-based tracking  
- **[@tanstack/electric-collection](https://github.com/TanStack/db/tree/main/packages/electric-collection)** - Pattern A: Transaction ID tracking with complex sync protocols
- **[@tanstack/rxdb-collection](https://github.com/TanStack/db/tree/main/packages/rxdb-collection)** - Pattern B: Built-in handlers that bridge [RxDB](https://rxdb.info) change streams into TanStack DB's sync lifecycle

### Key Lessons from Production Collections

**From Query Collection:**
- Simplest approach: Full refetch after mutations
- Best for: APIs without real-time sync
- Pattern: User provides `onInsert/onUpdate/onDelete` handlers

**From Trailbase Collection:**  
- Shows ID-based optimistic state management
- Handles provider batch limits (chunking large operations)
- Pattern: Collection provides mutation handlers using record API

**From Electric Collection:**
- Complex transaction ID tracking for distributed sync
- Demonstrates advanced deduplication techniques
- Shows how to wrap user handlers with sync coordination

**From RxDB Collection:**
- Uses RxDB's built-in queries and change streams
- Uses `RxCollection.$` to subscribe to inserts/updates/deletes and forward them to TanStack DB with begin-write-commit
- Implements built-in mutation handlers (onInsert, onUpdate, onDelete) that call RxDB APIs (bulkUpsert, incrementalPatch, bulkRemove)

## Complete Example: WebSocket Collection

Here's a complete example of a WebSocket-based collection options creator that demonstrates the full round-trip flow:

1. Client sends transaction with all mutations batched together
2. Server processes the transaction and may modify the data (validation, timestamps, etc.)
3. Server sends back acknowledgment and the actual processed data
4. Client waits for this round-trip before dropping optimistic state

```typescript
import type {
  CollectionConfig,
  SyncConfig,
  InsertMutationFnParams,
  UpdateMutationFnParams,
  DeleteMutationFnParams,
  UtilsRecord
} from '@tanstack/db'

interface WebSocketMessage<T> {
  type: 'insert' | 'update' | 'delete' | 'sync' | 'transaction' | 'ack'
  data?: T | T[]
  mutations?: Array<{
    type: 'insert' | 'update' | 'delete'
    data: T
    id?: string
  }>
  transactionId?: string
  id?: string
}

interface WebSocketCollectionConfig<TItem extends object>
  extends Omit<CollectionConfig<TItem>, 'onInsert' | 'onUpdate' | 'onDelete' | 'sync'> {
  url: string
  reconnectInterval?: number
  
  // Note: onInsert/onUpdate/onDelete are handled by the WebSocket connection
  // Users don't provide these handlers
}

interface WebSocketUtils extends UtilsRecord {
  reconnect: () => void
  getConnectionState: () => 'connected' | 'disconnected' | 'connecting'
}

export function webSocketCollectionOptions<TItem extends object>(
  config: WebSocketCollectionConfig<TItem>
): CollectionConfig<TItem> & { utils: WebSocketUtils } {
  let ws: WebSocket | null = null
  let reconnectTimer: NodeJS.Timeout | null = null
  let connectionState: 'connected' | 'disconnected' | 'connecting' = 'disconnected'
  
  // Track pending transactions awaiting acknowledgment
  const pendingTransactions = new Map<string, {
    resolve: () => void
    reject: (error: Error) => void
    timeout: NodeJS.Timeout
  }>()
  
  const sync: SyncConfig<TItem>['sync'] = (params) => {
    const { begin, write, commit, markReady } = params
    
    function connect() {
      connectionState = 'connecting'
      ws = new WebSocket(config.url)
      
      ws.onopen = () => {
        connectionState = 'connected'
        // Request initial sync
        ws.send(JSON.stringify({ type: 'sync' }))
      }
      
      ws.onmessage = (event) => {
        const message: WebSocketMessage<TItem> = JSON.parse(event.data)
        
        switch (message.type) {
          case 'sync':
            // Initial sync with array of items
            begin()
            if (Array.isArray(message.data)) {
              for (const item of message.data) {
                write({ type: 'insert', value: item })
              }
            }
            commit()
            markReady()
            break
            
          case 'insert':
          case 'update':
          case 'delete':
            // Real-time updates from other clients
            begin()
            write({ 
              type: message.type, 
              value: message.data as TItem 
            })
            commit()
            break
            
          case 'ack':
            // Server acknowledged our transaction
            if (message.transactionId) {
              const pending = pendingTransactions.get(message.transactionId)
              if (pending) {
                clearTimeout(pending.timeout)
                pendingTransactions.delete(message.transactionId)
                pending.resolve()
              }
            }
            break
            
          case 'transaction':
            // Server sending back the actual data after processing our transaction
            if (message.mutations) {
              begin()
              for (const mutation of message.mutations) {
                write({
                  type: mutation.type,
                  value: mutation.data
                })
              }
              commit()
            }
            break
        }
      }
      
      ws.onerror = (error) => {
        console.error('WebSocket error:', error)
        connectionState = 'disconnected'
      }
      
      ws.onclose = () => {
        connectionState = 'disconnected'
        // Auto-reconnect
        if (!reconnectTimer) {
          reconnectTimer = setTimeout(() => {
            reconnectTimer = null
            connect()
          }, config.reconnectInterval || 5000)
        }
      }
    }
    
    // Start connection
    connect()
    
    // Return cleanup function
    return () => {
      if (reconnectTimer) {
        clearTimeout(reconnectTimer)
        reconnectTimer = null
      }
      if (ws) {
        ws.close()
        ws = null
      }
    }
  }
  
  // Helper function to send transaction and wait for server acknowledgment
  const sendTransaction = async (
    params: InsertMutationFnParams<TItem> | UpdateMutationFnParams<TItem> | DeleteMutationFnParams<TItem>
  ): Promise<void> => {
    if (ws?.readyState !== WebSocket.OPEN) {
      throw new Error('WebSocket not connected')
    }
    
    const transactionId = crypto.randomUUID()
    
    // Convert all mutations in the transaction to the wire format
    const mutations = params.transaction.mutations.map(mutation => ({
      type: mutation.type,
      id: mutation.key,
      data: mutation.type === 'delete' ? undefined : 
           mutation.type === 'update' ? mutation.changes : 
           mutation.modified
    }))
    
    // Send the entire transaction at once
    ws.send(JSON.stringify({
      type: 'transaction',
      transactionId,
      mutations
    }))
    
    // Wait for server acknowledgment
    return new Promise<void>((resolve, reject) => {
      const timeout = setTimeout(() => {
        pendingTransactions.delete(transactionId)
        reject(new Error(`Transaction ${transactionId} timed out`))
      }, 10000) // 10 second timeout
      
      pendingTransactions.set(transactionId, {
        resolve,
        reject,
        timeout
      })
    })
  }
  
  // All mutation handlers use the same transaction sender
  const onInsert = async (params: InsertMutationFnParams<TItem>) => {
    await sendTransaction(params)
  }
  
  const onUpdate = async (params: UpdateMutationFnParams<TItem>) => {
    await sendTransaction(params)
  }
  
  const onDelete = async (params: DeleteMutationFnParams<TItem>) => {
    await sendTransaction(params)
  }
  
  return {
    id: config.id,
    schema: config.schema,
    getKey: config.getKey,
    sync: { sync },
    onInsert,
    onUpdate,
    onDelete,
    utils: {
      reconnect: () => {
        if (ws) ws.close()
        connect()
      },
      getConnectionState: () => connectionState
    }
  }
}
```

## Usage Example

```typescript
import { createCollection } from '@tanstack/react-db'
import { webSocketCollectionOptions } from './websocket-collection'

const todos = createCollection(
  webSocketCollectionOptions({
    url: 'ws://localhost:8080/todos',
    getKey: (todo) => todo.id,
    schema: todoSchema
    // Note: No onInsert/onUpdate/onDelete - handled by WebSocket automatically
  })
)

// Use the collection
todos.insert({ id: '1', text: 'Buy milk', completed: false })

// Access utilities
todos.utils.getConnectionState() // 'connected'
todos.utils.reconnect() // Force reconnect
```

## Advanced: Managing Optimistic State

A critical challenge in sync-first apps is knowing when to drop optimistic state. When a user makes a change:

1. The UI updates immediately (optimistic update)
2. A mutation is sent to the backend
3. The backend processes and persists the change
4. The change syncs back to the client
5. The optimistic state should be dropped in favor of the synced data

The key question is: **How do you know when step 4 is complete?**

### Strategy 1: Built-in Provider Methods (Recommended)

Many providers offer built-in methods to wait for sync completion:

```typescript
// Firebase
await waitForPendingWrites(firestore)

// Custom WebSocket
await websocket.waitForAck(transactionId)
```

### Strategy 2: Transaction ID Tracking (ElectricSQL)

ElectricSQL returns transaction IDs that you can track:

```typescript
// Track seen transaction IDs
const seenTxids = new Store<Set<number>>(new Set())

// In sync, track txids from incoming messages
if (message.headers.txids) {
  message.headers.txids.forEach(txid => {
    seenTxids.setState(prev => new Set([...prev, txid]))
  })
}

// Mutation handlers return txids and wait for them
const wrappedOnInsert = async (params) => {
  const result = await config.onInsert!(params)
  
  // Wait for the txid to appear in synced data
  if (result.txid) {
    await awaitTxId(result.txid)
  }
  
  return result
}

// Utility function to wait for a txid
const awaitTxId = (txId: number): Promise<boolean> => {
  if (seenTxids.state.has(txId)) return Promise.resolve(true)
  
  return new Promise((resolve) => {
    const unsubscribe = seenTxids.subscribe(() => {
      if (seenTxids.state.has(txId)) {
        unsubscribe()
        resolve(true)
      }
    })
  })
}
```

### Strategy 3: ID-Based Tracking (Trailbase)

Trailbase tracks when specific record IDs have been synced:

```typescript
// Track synced IDs with timestamps
const seenIds = new Store(new Map<string, number>())

// In sync, mark IDs as seen
write({ type: 'insert', value: item })
seenIds.setState(prev => new Map(prev).set(item.id, Date.now()))

// Wait for specific IDs after mutations
const wrappedOnInsert = async (params) => {
  const ids = await config.recordApi.createBulk(items)
  
  // Wait for all IDs to be synced back
  await awaitIds(ids)
}

const awaitIds = (ids: string[]): Promise<void> => {
  const allSynced = ids.every(id => seenIds.state.has(id))
  if (allSynced) return Promise.resolve()
  
  return new Promise((resolve) => {
    const unsubscribe = seenIds.subscribe((state) => {
      if (ids.every(id => state.has(id))) {
        unsubscribe()
        resolve()
      }
    })
  })
}
```

### Strategy 4: Version/Timestamp Tracking

Track version numbers or timestamps to detect when data is fresh:

```typescript
// Track latest sync timestamp
let lastSyncTime = 0

// In mutations, record when the operation was sent
const wrappedOnUpdate = async (params) => {
  const mutationTime = Date.now()
  await config.onUpdate(params)
  
  // Wait for sync to catch up
  await waitForSync(mutationTime)
}

const waitForSync = (afterTime: number): Promise<void> => {
  if (lastSyncTime > afterTime) return Promise.resolve()
  
  return new Promise((resolve) => {
    const check = setInterval(() => {
      if (lastSyncTime > afterTime) {
        clearInterval(check)
        resolve()
      }
    }, 100)
  })
}
```

### Strategy 5: Full Refetch (Query Collection)

The query collection simply refetches all data after mutations:

```typescript
const wrappedOnInsert = async (params) => {
  // Perform the mutation
  await config.onInsert(params)
  
  // Refetch the entire collection
  await refetch()
  
  // The refetch will trigger sync with fresh data,
  // automatically dropping optimistic state
}
```

### Choosing a Strategy

- **Built-in Methods**: Best when your provider offers sync completion APIs
- **Transaction IDs**: Best when your backend provides reliable transaction tracking
- **ID-Based**: Good for systems where each mutation returns the affected IDs
- **Full Refetch**: Simplest but least efficient; good for small datasets
- **Version/Timestamp**: Works when your sync includes reliable ordering information

### Implementation Tips

1. **Always wait for sync** in your mutation handlers to ensure optimistic state is properly managed
2. **Handle timeouts** - Don't wait forever for sync confirmation
3. **Clean up tracking data** - Remove old txids/IDs to prevent memory leaks
4. **Provide utilities** - Export functions like `awaitTxId` or `awaitSync` for advanced use cases

## Best Practices

1. **Always call markReady()** - This signals that the collection has initial data and is ready for use
2. **Handle errors gracefully** - Call markReady() even on error to avoid blocking the app
3. **Clean up resources** - Return a cleanup function from sync to prevent memory leaks
4. **Batch operations** - Use begin/commit to batch multiple changes for better performance
5. **Race Conditions** - Start listeners before initial fetch and buffer events
6. **Type safety** - Use TypeScript generics to maintain type safety throughout
7. **Provide utilities** - Export sync-engine-specific utilities for advanced use cases

## Testing Your Collection

Test your collection options creator with:

1. **Unit tests** - Test sync logic, data transformations
2. **Integration tests** - Test with real sync engine
3. **Error scenarios** - Connection failures, invalid data
4. **Performance** - Large datasets, frequent updates

## Conclusion

Creating a collection options creator allows you to integrate any sync engine with TanStack DB's powerful sync-first architecture. Follow the patterns shown here, and you'll have a robust, type-safe integration that provides excellent developer experience.
