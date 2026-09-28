# Reading and Writing a Group of Tags

## Context and Problem Statement

For a read, the framework gives the client a group of data points and expects a list of typed values back. For a
write, it gives a list of typed values. The write returns no result, so only an exception can report a failure. The
code is the read batch and the write batch in `src/AllenBradley.Logix/Client/`. This decision was made under
[issue #5: Client base design](https://gitlab.com/vicione-oss/addons/allen-bradley/cip/-/work_items/5).

The group has a different meaning in the two directions:

- A read group contains the data points with the same poll frequency. It exists only for the schedule and for
  performance, and it has no other meaning. The values of one read are not one consistent snapshot of the controller.
- A write batch contains the values that the engine gives in one cycle. These values belong together, for example a
  setpoint and its mode flag. For this reason, the outgoing port of the framework drops the full batch when one value
  fails conversion or range validation.

libplctag has no call that reads or writes many tags. Thus, a group is *N* separate handle operations. CIP also has no
transaction over several tags. Each tag write is a separate service, also inside one Multiple Service Packet. Thus, the
client cannot write a batch as one unit in the ACID sense.

libplctag can still send *N* operations in few round trips. All handles to one controller use one session with one
queue. The session thread packs all requests that wait in the queue into one Multiple Service Packet
([the shared session](../../AllenBradley.Documentation/libplctag/the-shared-session.md)). This occurs only when many
requests are in the queue at the same time. A loop that reads one tag after the other never packs.

Three questions follow. How does the client start the operations? What does it do when some tags fail and others do
not? How near can a write come to "all values or none"?

## Considered Options

1. **Resolve first, then start all.** Get the converter and the handle for each data point before any I/O. Then start
   one operation for each tag, all at the same time, and wait for all of them.
2. **Build the Multiple Service Packet ourselves** over `libplctag.NativeImport`.
3. **Copy the batch of the S7 dataport,** where one call of the client library reads the full group.

## Decision Outcome

Chosen option: **Option 1**. Concurrent operations fill the queue, and a full queue lets libplctag pack them. libplctag
has no batch call that we could use instead.

- Before any I/O, the batch gets a converter and a handle for each data point. A data point without a converter stops
  the batch before the controller gets a request.
- The write batch also encodes each value before any I/O. If a value cannot be encoded, the batch fails and sends
  nothing. The error names each value that failed.
- The batch starts all operations and waits for all of them. Each operation returns a result, not an exception.
  `Task.WhenAll` rethrows only the first exception, and the other failures would be lost.
- Cancellation throws, because the caller decides it.

### A read gives what it could read

A read group has no meaning beyond the schedule. Thus, one failed tag must not cost the values of the other tags. The client returns the
values of the tags that answered, and logs the failed tags as one warning. The next poll is already scheduled.

A read throws only when no tag answered. An empty list would make a dead controller look like an empty group. The
circuit breaker of the framework would then never open. An empty group does not throw.

A failed read does not give a placeholder value. The framework marks each value from the port as valid
(`Validity = 1`), so a placeholder would reach the engine as a valid null. A data point that is missing from the list
keeps its last value.

### A write makes a best effort to write the batch as one unit

The client cannot guarantee that a write batch arrives as one unit. It does these three things to come near:

1. If one value cannot be encoded, the batch sends nothing.
2. The batch starts all writes at the same time. Thus, the writes reach the controller close together in time, and
   libplctag can pack them into few packets.
3. If one tag write fails, the batch fails as a whole. One `LogixTagException` names each tag that failed. The
   framework keeps the full batch at the head of its queue and writes it again, until all values are written.

The client does not roll back the tags that were written before a failure. A rollback needs the old values, and a
rollback write can also fail.

For a write batch, the ACID properties are as follows:

| Property    | Write batch                                                                                                    |
|-------------|----------------------------------------------------------------------------------------------------------------|
| Atomicity   | Not guaranteed. After a failure, only a part of the batch is written. The retry of the full batch completes it later. |
| Consistency | In part. The framework and the encode step stop invalid values before any I/O. Verification at connect stops type mismatches. The client does not know the rules of the controller program. |
| Isolation   | Not guaranteed. The controller program, other clients and the read port can see a batch that is only partly written. The batches of one outgoing port do not overlap, because its queue writes one batch at a time. |
| Durability  | The controller keeps the values. The client has no part in it.                                                  |

### Consequences

- Good: libplctag packs the requests. We write no CIP encoding code.
- Good: Each failed tag is named with its reason. One failure does not hide the others.
- Good: One tag that cannot be read does not stop the values of the other tags in its group.
- Bad: The list from a read can be shorter than the group. A caller must not expect one value for each data point.
- Bad: The controller program can see a write batch that is only partly written. The writes in one batch also have no
  order.
- Bad: The retry writes again the values that were already written. This is safe for a tag that only the engine
  writes. It is not safe for a tag that the controller program also changes, for example a flag that the program resets.
- Open: There is no limit on the operations in flight. A group of 200 data points starts 200 operations. The limit
  must come from the throughput measurement
  ([Maximizing throughput with one shared connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md)).
- Open: The same measurement must confirm that libplctag packs concurrent operations. If it does not, this decision
  stays. The fallback is *N* round trips, or a batch call in a later libplctag version, but not an MSP of our own.

## Why Not the Other Options

### Option 2: Build the Multiple Service Packet ourselves

libplctag already packs the requests, so this gives nothing. It also needs a replacement for the full wrapper layer,
and [Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) rejects that. An MSP of our own
would also not make a write batch atomic, because each service in it is still a separate tag write.

### Option 3: Copy the S7 batch

The client library of the S7 dataport, S7.Net, has a call that reads many items in one request. libplctag has no such
call. Concurrent operations are the replacement.

## More Information

- Explanation: [Reading and writing](../explanation/client/reading-and-writing.md)
- Background: [The shared session](../../AllenBradley.Documentation/libplctag/the-shared-session.md)
- Framework: the outgoing data port in the `ViciOne.Suite.DataPort.Extensions` docs, for the write queue, the
  all-or-nothing validation and the retry of a full batch
- Related decisions: [Operations, not accessors](2026-07-16-operations-not-accessors-over-libplctag.md) ·
  [Maximizing throughput with one shared connection](2026-07-16-maximizing-throughput-with-one-shared-connection.md) ·
  [Reusing and releasing tag handles](2026-07-16-reusing-and-releasing-tag-handles.md) ·
  [Decoding tag bytes into typed values](2026-07-16-decoding-tag-bytes-into-typed-values.md)
